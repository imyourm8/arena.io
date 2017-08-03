using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using arena.Common;
using arena.battle.logic;
using arena.matchmaking;

using shared.net;
using shared.net.interfaces;
using shared.helpers;
using shared.database;
using shared.factories;

namespace arena.battle
{
    class Game : 
        IActionInvoker, 
        BlockSpawner.IBlockControl
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        private HashSet<Player> joinedPlayers_ = new HashSet<Player>();
        private HashSet<Unit> broadcastedUnits_ = new HashSet<Unit>();
        private Dictionary<int, Entity> entities_ = new Dictionary<int, Entity>();
        private Dictionary<int, Mob> mobs_ = new Dictionary<int, Mob>();
        private Dictionary<int, ExpBlock> expBlocks_ = new Dictionary<int, ExpBlock>();
        private Dictionary<int, Bullet> bullets_ = new Dictionary<int, Bullet>();
        private List<Bullet> bulletsToRemove_ = new List<Bullet>();
        private List<Player> players_ = new List<Player>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private List<KeyValuePair<Entity, Entity>> deathList_ = new List<KeyValuePair<Entity, Entity>>();
        private PoolFiber fiber_ = new PoolFiber(new DebugExecutor());
        private int id_ = 0;
        private modes.GameMode mode_;
        private long gameFinishAt_;
        private Room room_;
        private Map map_;
        private DetermenisticScheduler scheduler_ = new DetermenisticScheduler();
        private long prevUpdateTime_ = 0;
        private long startUpTime_ = 0;

        public long Time
        {
            get { return Tick * GlobalDefs.MainTickInterval; }
        }

        public int Tick
        {
            get;
            private set;
        }

        public Game(modes.GameMode mode, Room room)    
        {
            room_ = room; 
            gameFinishAt_ = CurrentTime.Instance.CurrentTimeInMs + mode.GetMatchDuration();

            startUpTime_ = CurrentTime.Instance.CurrentTimeInMs;
            prevUpdateTime_ = startUpTime_;

            mode_ = mode;
            mode_.Game = this; 

            var mapLoader = new MapLoader(mode_.GetMapPath(), this);    
            map_ = mapLoader.Load();

            scheduler_.SetStep(GlobalDefs.EventPoolInterval);      
            scheduler_.ScheduleOnInterval(HandleMainUpdate, GlobalDefs.MainTickInterval);
            scheduler_.ScheduleOnInterval(HandleAIUpdate, GlobalDefs.AITickInterval);
            scheduler_.ScheduleOnInterval(BroadcastEntities, GlobalDefs.BroadcastEntitiesInterval);
            scheduler_.ScheduleOnInterval(SendPlayerStatuses, 500);

            fiber_.ScheduleOnInterval(HandleUpdate, 0, GlobalDefs.EventPoolInterval);
            //fiber_.ScheduleOnInterval(SendPlayerStatuses, 500, 500);
            fiber_.ScheduleOnInterval(UpdateMap, 0, 15000);
            //fiber_.ScheduleOnInterval(BroadcastEntities, 0, GlobalDefs.BroadcastEntitiesInterval);
            //fiber_.ScheduleOnInterval(HandleAIUpdate, GlobalDefs.AITickInterval, GlobalDefs.AITickInterval);
            fiber_.Schedule(FinishGame, mode.GetMatchDuration());

            fiber_.Start();
        }

        private void BroadcastEntities()
        {
            var unitStates = new proto_game.UnitStatesUpdate();
            foreach (var unit in broadcastedUnits_)
            {
                unitStates.states.Add(unit.GetSerializedState());
            }
            unitStates.tick = Tick;
            unitStates.timestamp = CurrentTime.Instance.CurrentTimeInMs;
            Broadcast(proto_common.Events.UNIT_STATE_UPDATE, unitStates);
        }

        private void HandleAIUpdate() 
        {
            var dt = GlobalDefs.GetAIUpdateInterval();  
            foreach (var pair in mobs_)
            {
                pair.Value.Update(dt); 
            }

            //world_.Step(dt, 1, 1);

            foreach (var pair in mobs_)
            {
                //pair.Value.PostUpdate();
            }
        }

        private void HandleUpdate()  
        {
            var t = CurrentTime.Instance.CurrentTimeInMs;
            long dt = (t - prevUpdateTime_);
            scheduler_.Update(dt);  
            prevUpdateTime_ = CurrentTime.Instance.CurrentTimeInMs;
        }

        public Map Map
        { get { return map_; } }  

        public int GenerateID()
        {
            return id_++;
        }

        public int GetCurrentEntityID()
        {
            return id_;
        }

        private void UpdateMap()
        {
            map_.Update();
        }

        public void ConnectPlayer(Player player)
        {
            player.Game = this;
            player.BattleStats.Reset();
            player.ResetLevel();

            fiber_.Enqueue(() =>
            {
                //add to players pool 
                players_.Add(player);
            });
        }

        public void RegisterAsDead(Entity killer, Entity victim)  
        {
            deathList_.Add(new KeyValuePair<Entity,Entity>(killer, victim));
        }

        public int MaxTickDelta
        {
            get { return (int)(1.0f / GlobalDefs.GetUpdateInterval() * 1.0f); }
        }

        public void DamageApply(Player player, proto_game.DamageApply.Request damageData)
        {
            var target = GetEntity(damageData.target);
            var attacker = GetEntity(damageData.attacker) as Unit;

            if (attacker == null || target == null)
            {
                return;
            }

            if (!attacker.IsAlive || !attacker.IsOwnerOf(damageData.bullet))
            {
                return;
            }

            target.ApplyDamage(attacker, damageData.damage);
            var bulletsRemovedPacket = new proto_game.BulletsRemoved();
            bulletsRemovedPacket.guid.Add(damageData.bullet); 
            Broadcast(proto_common.Events.BULLET_DESTROYED, bulletsRemovedPacket, player);
        }

        public void PlayerJoin(Player player)
        {
            fiber_.Enqueue(() =>
            {
                //player.ID = GenerateID();
                mode_.SpawnPlayer(player);
                player.AssignStats();
                Add(player);

                //send all mobs
                foreach (var e in mobs_)
                {
                    player.Controller.SendEvent(e.Value.GetAppearedPacket());
                }

                //send all powerups
                foreach (var e in powerUps_)
                {
                    player.Controller.SendEvent(e.GetAppearedPacket());
                }

                //send all exp blocks
                foreach (var e in expBlocks_)
                {
                    player.Controller.SendEvent(e.Value.GetAppearedPacket());
                }

                //send packet to connected player aswell
                //TODO remove this part, client should track everything by itself
                var appearedPacket = player.GetAppearedPacket(); 
                (appearedPacket.Packet as proto_game.PlayerAppeared).local = true;
                player.Controller.SendEvent(appearedPacket);

                joinedPlayers_.Add(player);

                //send join game packet wit map id
                var joinPacket = new proto_game.JoinGame.Response();
                joinPacket.map_id = mode_.GetMapID();
                player.Controller.SendResponse(proto_common.Commands.JOIN_GAME, joinPacket); 
            });
        }

        public Bullet SpawnBullet(proto_game.Bullets bulletId, Unit owner)
        {
            var bullet = new Bullet();

            bullet.Owner = owner;
            bullet.Entry = BulletFactory.Instance.GetEntry(bulletId);
            bullet.Stats.SetValue(proto_game.Stats.MovementSpeed, bullet.Entry.Speed);
            bullet.Radius = bullet.Entry.Radius;

            Add(bullet);
            bullets_.Add(bullet.ID, bullet);
            return bullet;
        }

        public void Remove(Bullet bullet)
        {
            bulletsToRemove_.Add(bullet);
        }

        private void PlayerDead(Player player)
        {
            fiber_.Enqueue(() =>
            {
                foreach (var powerUp in powerUps_)
                {
                    if (powerUp.Holder == player)
                    {
                        //drop powerUp
                        powerUp.Holder = null;
                        Broadcast(powerUp.GetAppearedPacket());
                    }
                }

                if (player.Level >= 6)
                {
                    var levelCut = player.Level / 3;
                    player.Level -= levelCut;
                }
                else
                {
                    player.ResetLevel();
                }

                player.Exp = 0;
                joinedPlayers_.Remove(player);
                Remove((Entity)player);
            });
        }

        public void Remove(Player player)
        {
            fiber_.Enqueue(() =>
            {
                players_.Remove(player);
                joinedPlayers_.Remove(player);
                player.Game = null;
                Remove((Entity)player);
            });
        }

        public void PlayerAttacked(Player player, proto_game.UnitAttack attackData)
        {
            Broadcast(proto_common.Events.UNIT_ATTACK, attackData, player);
        }

        public void OnSkillCast(Unit unit, int bullet_id)
        {
            var castEvt = new proto_game.SkillCasted();
            castEvt.direction = unit.Rotation;

            var pos = unit.Position;
            castEvt.x = pos.x;
            castEvt.y = pos.y;
            castEvt.tick = Tick;
            castEvt.guid = unit.ID;
            castEvt.first_bullet_id = bullet_id;

            Broadcast(proto_common.Events.SKILL_CASTED, castEvt);
        }

        public void SyncAttackWithRemotePlayer(Player player, int firstBulletId, int inputID)
        {
            proto_game.SyncAttack syncAttack = new proto_game.SyncAttack();
            syncAttack.sync_id = inputID;
            syncAttack.first_bullet_id = firstBulletId;

            if (player != null)
            {
                player.Controller.SendEvent(proto_common.Events.SYNC_ATTACK, syncAttack);
            }
        }

        public void OnSpawnBullets(
            int owner, 
            float direction, 
            float angleStep, 
            int shotCount, 
            Vector2 pos, 
            float damage, 
            int startId,
            proto_game.Bullets bulletType)
        {
            var evt = new proto_game.SpawnBullets();
            evt.angleStep = angleStep;
            evt.direction = direction;
            evt.owner = owner;
            evt.startId = startId;
            evt.x = pos.x;
            evt.y = pos.y;
            evt.count = shotCount;
            evt.damage = damage;
            evt.tick = Tick;
            evt.type = bulletType;

            Broadcast(proto_common.Events.SPAWN_BULLETS, evt);
        }

        public void OnUnitWeaponAttack(Unit unit, AttackData attack)
        {
            proto_game.UnitAttack attackData = new proto_game.UnitAttack();
            attackData.direction = attack.Direction;
            attackData.guid = unit.ID;
            attackData.time_advance = 0;
            attackData.first_bullet_id = attack.FirstBulletID;
            attackData.tick = Tick;

            var pos = unit.Position;
            attackData.x = pos.x;
            attackData.y = pos.y;

            var player = unit as Player;
            Broadcast(proto_common.Events.UNIT_ATTACK, attackData, player);
        }

        public void OnCoinPickedUp(Player player, GoldCoin coin)
        {
            player.Profile.AddCoins(coin.Value);
            Remove(coin);

            var evt = new proto_game.PlayerCoinsChange();
            evt.coins = player.Profile.Coins;
            player.Controller.SendEvent(proto_common.Events.PLAYER_COINS, evt);
        }

        private void ProcessKill(Entity target)
        {
            ExpBlock expBlock = target as ExpBlock;
            if (expBlock != null)
            {
                map_.OnExpBlockRemoved(expBlock);
                SpawnGoldPiles(expBlock);
                return;
            }

            Mob mob = target as Mob;
            if (mob != null)
            {
                map_.OnMobDead(mob);
                return;
            }

            Player player = target as Player;
            if (player != null)
            {
                PlayerDead(player);
                return;
            }
        }

        private void ProcessDeathList()
        {
            foreach (var pair in deathList_)
            {
                var killer = pair.Key;
                var target = pair.Value;
                var player = killer as Player;

                if (player != null)
                    mode_.HandleEntityKill(player, target);

                int expGenerated = target.Exp;
                int score = 0;
                int coins = 0;

                ProcessKill(target);

                if (player != null)
                {
                    var victim = target as Player;
                    if (victim != null)
                    {
                        player.BattleStats.Kills++;
                        expGenerated = mode_.GetExpFor(player, victim);
                        coins = mode_.GetCoinsFor(player, victim);
                        score = mode_.GetScoreFor(player, victim);
                    }

                    player.BattleStats.Score += score;
                    player.BattleStats.Coins += coins;
                    player.AddExperience(expGenerated);

                    var expPacket = new proto_game.PlayerExperience(); 
                    expPacket.exp = expGenerated;
                    player.Controller.SendEvent(proto_common.Events.PLAYER_EXPERIENCE, expPacket);
                }
            }

            deathList_.Clear();
        }

        private void SpawnGoldPiles(ExpBlock block)
        {
            float spawnRadius = block.Radius * 1.5f;

            for (int i = 0; i < block.Coins; ++i)
            {
                var goldCoin = new GoldCoin();
                goldCoin.PickupType = proto_game.Pickups.SmallGoldCoin;
                goldCoin.Value = 1;

                var pos = MathHelper.RandomPointInsideCircle(block.Position, spawnRadius);
                goldCoin.Position = pos;
                Add(goldCoin);

                var goldCoinPacket = new proto_game.SpawnPickUp();
                goldCoinPacket.x = pos.x;
                goldCoinPacket.y = pos.y;
                goldCoinPacket.guid = goldCoin.ID;
                goldCoinPacket.type = proto_game.Pickups.SmallGoldCoin;
                Broadcast(proto_common.Events.SPAWN_PICK_UP, goldCoinPacket);
            }
        }

        public void Add(Mob mob)
        {
            Add((Entity)mob);
            mobs_.Add(mob.ID, mob);
        }

        public void Remove(Mob mob)
        {
            mobs_.Remove(mob.ID);
            Remove((Entity)mob);
        }

        public void Remove(Entity entity) 
        {
            entities_.Remove(entity.ID);
            map_.Remove(entity);

            if (entity is Unit)
            {
                broadcastedUnits_.Remove(entity as Unit);
            }

            Execute(() =>
                {
                    if (entity.ReplicateOnClients)
                    {
                        var removePacket = new proto_game.EntityRemoved();
                        removePacket.guid = entity.ID;
                        Broadcast(proto_common.Events.ENTITY_REMOVED, removePacket);
                    }
                });
        }

        private void Add(Entity entity)
        {
            entity.ID = GenerateID();
            entities_.Add(entity.ID, entity);
            map_.Add(entity);
            entity.Game = this;
            entity.InitPhysics();

            if (entity is Unit)
            {
                broadcastedUnits_.Add(entity as Unit);
            }

            Broadcast(entity.GetAppearedPacket());
        }

        void BlockSpawner.IBlockControl.AddBlock(ExpBlock block)
        {
            Add(block);
            expBlocks_.Add(block.ID, block);
        }

        void BlockSpawner.IBlockControl.RemoveBlock(ExpBlock block)
        {
            Remove(block);
            expBlocks_.Remove(block.ID);
        }

        private Entity GetEntity(int id)
        {
            Entity entity;
            entities_.TryGetValue(id, out entity);
            return entity;
        }

        private void SendPlayerStatuses()
        {
            foreach (var player in joinedPlayers_)
            {
                if (System.Math.Abs(player.HP - player.Stats.GetFinValue(proto_game.Stats.MaxHealth)) > 0.001f) 
                    // make fields as classes to utilize message compressing
                    Broadcast(proto_common.Events.PLAYER_STATUS_CHANGED, player.GetStatusChanged());
            }
        }

        private void HandleMainUpdate()
        {
            float dt = GlobalDefs.GetUpdateInterval();

            foreach (var plr in joinedPlayers_)
            {
                plr.Update(dt);
                plr.ProcessInput(dt);
            }

            map_.StepPhysics(dt);

            var playerMoveResponse = new proto_game.PlayerInput.Response();
            foreach (var plr in joinedPlayers_)
            {
                plr.PostUpdate();

                if (plr.Input == null)
                    continue;

                //send sync packet
                var input = plr.Input.data;
                playerMoveResponse.tick = input.tick;
                playerMoveResponse.force_x = input.force_x;
                playerMoveResponse.force_y = input.force_y;
                playerMoveResponse.x = plr.Body.Position.X;
                playerMoveResponse.y = plr.Body.Position.Y;
                playerMoveResponse.shoot = input.shoot;
                playerMoveResponse.skill = input.skill;

                plr.Controller.SendResponse(proto_common.Commands.PLAYER_INPUT, playerMoveResponse);
            }

            foreach (var bullet in bullets_)
            {
                bullet.Value.Update(dt);
            }

            foreach (var bullet in bulletsToRemove_)
            {
                bullets_.Remove(bullet.ID);
                Remove((Entity)bullet);
            }
            bulletsToRemove_.Clear();
                
            List<PowerUp> toRemove = ListPool<PowerUp>.Get();
            foreach (var pwr in powerUps_)
            {
                if (pwr.Holder != null)
                {
                    pwr.Lifetime -= (int)GlobalDefs.MainTickInterval;
                    if (pwr.Lifetime <= 0)
                    {
                        toRemove.Add(pwr);
                    }
                }
            }

            foreach (var pwr in toRemove)
            {
                powerUps_.Remove(pwr);
                Remove(pwr);
            }
            ListPool<PowerUp>.Release(toRemove);

            ProcessDeathList();

            mode_.Update(dt);
            Tick++;
        }

        public void Broadcast(Net.EventPacket packet, Player exceptThis = null)
        {
            if (packet.IsValid)
                Broadcast(packet.EventID, packet.Packet, exceptThis);
        }

        public void Broadcast(proto_common.Events evtKey, object msg, Player exceptThis = null)
        {
            foreach (var player in joinedPlayers_)
            {
                if (exceptThis == player) 
                    continue;

                player.Controller.SendEvent(evtKey, msg);
            }
        }

        public void AddPowerUp(PowerUp powerUp)
        {
            powerUps_.Add(powerUp);
            Add(powerUp);
        }

        public void TryGrabPowerUp(int powerUpId, Player player)
        {
            foreach (var pwr in powerUps_)
            {
                if (pwr.ID == powerUpId && pwr.Holder == null)
                {
                    pwr.Holder = player;  

                    var grabEvt = new proto_game.PowerUpGrabbed();
                    grabEvt.who_grabbed = player.ID;
                    grabEvt.id = powerUpId;
                    Broadcast(proto_common.Events.POWER_UP_GRABBED, grabEvt);
                    break;
                }
            }
        }

        public void Execute(Action action)
        {
            fiber_.Enqueue(action);
        }

        private void FinishGame()
        {
            proto_game.GameFinished finishPacket = new proto_game.GameFinished();

            foreach (var player in players_)
            {
                finishPacket.exp = player.Exp;
                finishPacket.coins = player.BattleStats.Coins;
                //apply new data to player's profile 
                player.Profile.AddExperience(finishPacket.exp);
                player.Profile.AddCoins(finishPacket.coins);
                //add battle stats for all-time statistics
                player.Controller.SaveToDB();
                //send data
                player.Controller.SendEvent(proto_common.Events.GAME_FINISHED, finishPacket);
                //kick
                Remove(player);
            }  

            room_.OnGameFinished();
        }

        public void Close()
        {
            players_.Clear();
            fiber_.Dispose(); 
            entities_.Clear();
            mode_ = null;
            map_.Clear();
        }
    }
}
