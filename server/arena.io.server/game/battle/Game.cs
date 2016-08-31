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
using arena.helpers;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;
using Box2DX.Dynamics.Controllers;

namespace arena.battle
{
    class Game : player.IActionInvoker, BlockSpawner.IBlockControl
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        private HashSet<Player> joinedPlayers_ = new HashSet<Player>();
        private Dictionary<int, Entity> units_ = new Dictionary<int, Entity>();
        private Dictionary<int, Mob> mobs_ = new Dictionary<int, Mob>();
        private Dictionary<int, Bullet> bullets_ = new Dictionary<int, Bullet>();
        private List<Bullet> bulletsToRemove_ = new List<Bullet>();
        private List<Player> players_ = new List<Player>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private List<KeyValuePair<Entity, Entity>> deathList_ = new List<KeyValuePair<Entity, Entity>>();
        private PoolFiber fiber_ = new PoolFiber();
        private int id_ = 0;
        private GameModes.GameMode mode_;
        private long matchFinishAt_;
        private Room room_;
        private Map map_;
        private World pWorld_;
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

        public Game(GameModes.GameMode mode, Room room)    
        {
            room_ = room; 
            matchFinishAt_ = CurrentTime.Instance.CurrentTimeInMs + mode.GetMatchDuration();

            startUpTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
            prevUpdateTime_ = startUpTime_;

            mode_ = mode;
            mode_.Game = this; 

            var mapLoader = new MapLoader(mode_.GetMapPath(), this);    
            map_ = mapLoader.Load();

            var worldAABB = new AABB();
            var mapArea = map_.GetOuterBorder();
            worldAABB.LowerBound = new Vec2(mapArea[0], mapArea[1]); 
            worldAABB.UpperBound = new Vec2(mapArea[2], mapArea[3]);
            pWorld_ = new World(worldAABB, new Vec2(0, 0), true);
            CreateWallsAroundWorld(worldAABB);

            var bulletListenerAndFilter = new GameContactListener(); 
            pWorld_.SetContactListener(bulletListenerAndFilter);
            pWorld_.SetContactFilter(bulletListenerAndFilter);

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

        private void CreateWallsAroundWorld(AABB worldAABB)
        {
            var bodyDef = new BodyDef(); 
            var body = pWorld_.CreateBody(bodyDef);

            var edge = new EdgeDef(); 
            edge.Density = 0.0f;
            edge.Filter.CategoryBits = (ushort)PhysicsDefs.Category.WALLS;
            edge.Filter.MaskBits = 
                (ushort)PhysicsDefs.Category.EXP_BLOCK|(ushort)PhysicsDefs.Category.PLAYER|(ushort)PhysicsDefs.Category.MOB;

            //shrink need to prevent physics object stuck in world's bounds, dunno why it happends
            float shrinkAABB = 0.1f;
            edge.Vertex1.Set(worldAABB.LowerBound.X + shrinkAABB, worldAABB.UpperBound.Y - shrinkAABB);
            edge.Vertex2.Set(worldAABB.UpperBound.X - shrinkAABB, worldAABB.UpperBound.Y - shrinkAABB);
            body.CreateFixture(edge);

            edge.Vertex1.Set(worldAABB.UpperBound.X - shrinkAABB, worldAABB.UpperBound.Y - shrinkAABB);
            edge.Vertex2.Set(worldAABB.UpperBound.X + shrinkAABB, worldAABB.LowerBound.Y - shrinkAABB);
            body.CreateFixture(edge);

            edge.Vertex1.Set(worldAABB.UpperBound.X + shrinkAABB, worldAABB.LowerBound.Y - shrinkAABB);
            edge.Vertex2.Set(worldAABB.LowerBound.X + shrinkAABB, worldAABB.LowerBound.Y + shrinkAABB);
            body.CreateFixture(edge); 

            edge.Vertex1.Set(worldAABB.LowerBound.X + shrinkAABB, worldAABB.LowerBound.Y + shrinkAABB);
            edge.Vertex2.Set(worldAABB.LowerBound.X + shrinkAABB, worldAABB.UpperBound.Y - shrinkAABB);
            body.CreateFixture(edge);
        }

        private void BroadcastEntities()
        {
            var unitStates = new proto_game.UnitStatesUpdate();        
            foreach (var mobData in mobs_)
            {
                var unitStatePacket = new proto_game.UnitState();         
                var mob = mobData.Value;
                var pos = mob.Position;
                unitStatePacket.guid = mob.ID;
                unitStatePacket.x = pos.x;
                unitStatePacket.y = pos.y;
                unitStatePacket.rotation = mob.Rotation;              
                unitStates.states.Add(unitStatePacket);
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

            pWorld_.Step(dt, 1, 1);

            foreach (var pair in mobs_)
            {
                pair.Value.PostUpdate();
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

        public void Add(Player player)
        {
            player.Game = this;
            player.BattleStats.Reset();

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
            get { return (int)(1.0f / GlobalDefs.GetUpdateInterval() * 0.6f); }
        }

        public void DamageApply(Player player, proto_game.DamageApply.Request damageData)
        {
            GetEntity(damageData.target).ApplyDamage(GetEntity(damageData.attacker), damageData.damage);
            var bulletsRemovedPacket = new proto_game.BulletsRemoved();
            bulletsRemovedPacket.guid.Add(damageData.bullet); 
            Broadcast(proto_common.Events.BULLET_DESTROYED, bulletsRemovedPacket, player);
        }

        public void PlayerJoin(Player player)
        {
            fiber_.Enqueue(() =>
            {
                player.ID = GenerateID();
                mode_.SpawnPlayer(player);
                map_.Add(player);
                player.AssignStats();

                //broadcast new player across other players
                var appearedPacket = player.GetAppearedPacket(); 
                Broadcast(proto_common.Events.PLAYER_APPEARED, appearedPacket, player); 

                //now gather all online players now and send to new player
                foreach (var p in joinedPlayers_)
                {
                    player.Controller.SendEvent(proto_common.Events.PLAYER_APPEARED, p.GetAppearedPacket());
                }

                //send blocks, mobs, etc
                foreach (var e in units_)
                {
                    if (e.Value is ExpBlock)
                    {
                        player.Controller.SendEvent(proto_common.Events.BLOCK_APPEARED, (e.Value as ExpBlock).GetAppearedPacket());
                    }
                    else if (e.Value is Mob)
                    {
                        player.Controller.SendEvent(proto_common.Events.MOB_APPEARED, (e.Value as Mob).GetAppearedPacket());
                    }
                }

                //send packet to connected player aswell
                appearedPacket.local = true;
                player.Controller.SendEvent(proto_common.Events.PLAYER_APPEARED, appearedPacket);

                Add((Entity)player);
                player.InitPhysics();
                joinedPlayers_.Add(player);

                //send join game packet
                var joinPacket = new proto_game.JoinGame.Response();
                var outerBorder = map_.GetOuterBorder();
                foreach (var coord in outerBorder)
                {
                    joinPacket.outer_border.Add(coord); 
                }
                joinPacket.tick = Tick;
                joinPacket.time_left = (int)(matchFinishAt_ - Time);
                player.Controller.SendResponse(proto_common.Commands.JOIN_GAME, joinPacket); 
            });
        }

        public Body CreateBody(BodyDef def) 
        {
            return pWorld_.CreateBody(def);
        }

        public Bullet SpawnBullet(proto_game.Bullets bulletId, Unit owner)
        {
            var bullet = new Bullet();
            bullet.ID = GenerateID();
            bullets_.Add(bullet.ID, bullet);
            Add(bullet);

            bullet.Owner = owner;
            bullet.Entry = Factories.BulletFactory.Instance.GetEntry(bulletId);
            bullet.Stats.SetValue(proto_game.Stats.MovementSpeed, bullet.Entry.Speed);
            bullet.Radius = bullet.Entry.Radius;
            bullet.InitPhysics(true, true);

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
                        Broadcast(proto_common.Events.POWER_UP_APPEARED, powerUp.GetPowerUpPacket());
                    }
                }

                if (player.Level > 10)
                {
                    var levelCut = player.Level / 3;
                    player.Level -= levelCut;
                }
                else
                {
                    player.Level = 1;
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

                Broadcast(proto_common.Events.PLAYER_DISCONNECTED, player.GetDisconnectedPacket(), player);
            });
        }

        public void PlayerAttacked(Player player, proto_game.UnitAttack attackData)
        {
            Broadcast(proto_common.Events.UNIT_ATTACK, attackData, player);
        }

        public void OnSkillCast(Unit unit)
        { 
             
        }

        public void SyncAttackWithRemotePlayer(Player player, AttackData attData, int inputTick)
        {
            proto_game.UnitAttack attackData = new proto_game.UnitAttack();
            attackData.direction = attData.Direction;
            attackData.guid = player.ID;
            attackData.time_advance = 0;
            attackData.first_bullet_id = attData.FirstBulletID;
            attackData.tick = inputTick;
            attackData.local = true;

            if (player != null)
            {
                attackData.local = true;
                attackData.time_advance = System.Math.Max(0, player.Ping);
                player.Controller.SendEvent(proto_common.Events.UNIT_ATTACK, attackData);
            }
        }

        public void OnUnitAttack(Unit unit, AttackData attack)
        {
            proto_game.UnitAttack attackData = new proto_game.UnitAttack();
            attackData.direction = attack.Direction;
            attackData.guid = unit.ID;
            attackData.time_advance = 0;
            attackData.first_bullet_id = attack.FirstBulletID;
            attackData.tick = Tick;
            attackData.local = false;

            var pos = unit.Position;
            attackData.x = pos.x;
            attackData.y = pos.y;

            var player = unit as Player;
            Broadcast(proto_common.Events.UNIT_ATTACK, attackData, player);
        }

        public void PlayerTurned(Player player, proto_game.PlayerTurn turnData)
        {
            Broadcast(proto_common.Events.PLAYER_TURN, turnData, player);
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

                Broadcast(proto_common.Events.UNIT_DIE, target.GetDiePacket());

                int expGenerated = target.Exp;
                int score = 0;
                int gold = 0;
                
                if (target is ExpBlock)
                {
                    map_.OnExpBlockRemoved(target);
                    SpawnGoldPiles(target as ExpBlock); 
                }
                else if (target is Mob)
                {
                    map_.OnMobDead(target as Mob);
                }

                if (player != null)
                {
                    if (target is Player)
                    {
                        var targetPlayer = target as Player;
                        PlayerDead(targetPlayer);

                        var lvlDiff = System.Math.Max(1, targetPlayer.Level - player.Level);
                        expGenerated = 10 * targetPlayer.Level * lvlDiff;
                        score = targetPlayer.BattleStats.Score / 10;
                        player.BattleStats.Kills++;
                        gold += targetPlayer.Level / 10;
                    }

                    player.BattleStats.Score += score;
                    player.BattleStats.Gold += gold;

                    var expPacket = new proto_game.PlayerExperience(); 
                    expPacket.exp = expGenerated;
                    player.AddExperience(expGenerated);
                    player.Controller.SendEvent(proto_common.Events.PLAYER_EXPERIENCE, expPacket);
                }
            }

            deathList_.Clear();
        }

        private void SpawnGoldPiles(ExpBlock block)
        {
            var piles = (int)System.Math.Ceiling((float)block.Coins / 10.0f); //10 gold per pile
            for (int i = 0; i < piles; ++i)
            {
                
            }
        }

        public void Add(Mob mob)
        {
            mob.ID = GenerateID();
            mobs_.Add(mob.ID, mob);
            Add((Entity)mob);

            Broadcast(proto_common.Events.MOB_APPEARED, mob.GetAppearedPacket());
        }

        public void Remove(Mob mob)
        {
            mobs_.Remove(mob.ID);
            Remove((Entity)mob);
        }

        private void Remove(Entity mob)
        {
            units_.Remove(mob.ID);
            map_.Remove(mob);

            if (mob.Body != null)
            {
                pWorld_.DestroyBody(mob.Body);
                mob.Body = null;
            }
        }

        private void Add(Entity unit)
        {
            units_.Add(unit.ID, unit);
            map_.Add(unit);
            unit.Game = this;
        }

        void BlockSpawner.IBlockControl.AddBlock(ExpBlock block)
        {
            block.ID = GenerateID();
            Add(block); 
            Broadcast(proto_common.Events.BLOCK_APPEARED, block.GetAppearedPacket());
        }

        void BlockSpawner.IBlockControl.RemoveBlock(ExpBlock block)
        {
            Remove(block);
        }

        private Entity GetEntity(int id)
        {
            Entity entity;
            units_.TryGetValue(id, out entity);
            return entity;
        }

        private void SendPlayerStatuses()
        {
            foreach (var player in joinedPlayers_)
            {
                if (System.Math.Abs(player.HP - player.Stats.GetFinValue(proto_game.Stats.MaxHealth)) > 0.001f) 
                    //make fields as classes to utilize message packing
                    Broadcast(proto_common.Events.PLAYER_STATUS_CHANGED, player.GetStatusChanged());
            }
        }

        private bool HasInputs()
        {
            bool hasInput = false;
            foreach (var plr in players_)
            {
                hasInput |= plr.HasInput();
            }
            return hasInput;
        }

        private void HandleMainUpdate()
        {
            float dt = GlobalDefs.GetUpdateInterval();

            while (HasInputs())
            {
                foreach (var plr in joinedPlayers_)
                {
                    plr.Update(dt);
                    plr.ProcessInput(dt);
                }

                pWorld_.Step(dt, 1, 1);

                var playerMoveResponse = new proto_game.PlayerInput.Response();
                foreach (var plr in joinedPlayers_)
                {
                    plr.PostUpdate();

                    if (plr.Input == null)
                        continue;

                    //send sync packet
                    playerMoveResponse.tick = plr.Input.tick;
                    playerMoveResponse.force_x = plr.Input.force_x;
                    playerMoveResponse.force_y = plr.Input.force_y;
                    playerMoveResponse.x = plr.Body.GetPosition().X;
                    playerMoveResponse.y = plr.Body.GetPosition().Y;
                    playerMoveResponse.shoot = plr.Input.shoot;
                    playerMoveResponse.skill = plr.Input.skill;

                    plr.Controller.SendResponse(proto_common.Commands.PLAYER_INPUT, playerMoveResponse);
                }

                /*
                foreach (var bullet in bullets_)
                {
                    bullet.Value.Update(dt);
                }

                var bulletsRemovedPacket = new proto_game.BulletsRemoved();
                foreach (var bullet in bulletsToRemove_)
                {
                    bullets_.Remove(bullet.ID);
                    Remove((Entity)bullet);
                    bulletsRemovedPacket.guid.Add(bullet.ID);
                }
                bulletsToRemove_.Clear();
                Broadcast(proto_common.Events.BULLET_DESTROYED, bulletsRemovedPacket);
                */
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
            };

            mode_.Update(dt);
            Tick++;
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
            powerUp.ID = GenerateID();
            powerUps_.Add(powerUp);
            Add(powerUp);
            Broadcast(proto_common.Events.POWER_UP_APPEARED, powerUp.GetPowerUpPacket());
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

                    player.AddStatus(pwr.Type, (float)pwr.Lifetime / 1000.0f);
                    break;
                }
            }
        }

        public void SpawnPowerUp(proto_game.PowerUpType type, float x, float y, int lifetime)
        {
            PowerUp pwr = new PowerUp();
            pwr.ID = GenerateID();
            pwr.Type = type;
            pwr.Lifetime = lifetime;

            var evt = new proto_game.PowerUpAppeared();
            evt.type = type;
            evt.x = x;
            evt.y = y;

            Broadcast(proto_common.Events.POWER_UP_APPEARED, evt);

            powerUps_.Add(pwr);
            Add(pwr);
        }

        void player.IActionInvoker.Execute(Action action)
        {
            fiber_.Enqueue(action);
        }

        private void FinishGame()
        {
            proto_game.GameFinished finishPacket = new proto_game.GameFinished();

            foreach (var player in players_)
            {
                finishPacket.exp = mode_.GetExpFor(player);
                finishPacket.coins = mode_.GetCoinsFor(player);
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
            units_.Clear();
            mode_ = null;
            map_.Clear();
        }
    }
}
