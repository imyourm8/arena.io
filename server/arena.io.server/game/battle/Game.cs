using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ExitGames.Concurrency.Fibers;

using arena.helpers;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;
using Box2DX.Dynamics.Controllers;

namespace arena.battle
{
    class Game : player.IActionInvoker, BlockSpawner.IBlockControl
    {
        private HashSet<Player> joinedPlayers_ = new HashSet<Player>();
        private Dictionary<int, Entity> units_ = new Dictionary<int, Entity>();//use concurrent dictionary
        private Dictionary<int, Mob> mobs_ = new Dictionary<int, Mob>();
        private Dictionary<int, Bullet> bullets_ = new Dictionary<int, Bullet>();
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
        private long accumulator_ = 0;

        public long Time
        {
            get;
            private set;
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

            Time = helpers.CurrentTime.Instance.CurrentTimeInMs;

            mode_ = mode;
            mode_.Game = this; 

            var mapLoader = new MapLoader(mode_.GetMapPath(), this);    
            map_ = mapLoader.Load();

            var worldAABB = new AABB();
            var mapArea = map_.GetOuterBorder();
            worldAABB.LowerBound = new Vec2(mapArea[0], mapArea[1]);
            worldAABB.UpperBound = new Vec2(mapArea[2], mapArea[3]);
            pWorld_ = new World(worldAABB, new Vec2(0, 0), true);
            //pWorld_.SetContinuousPhysics(false);

            var bulletListenerAndFilter = new BulletContactListener();
            pWorld_.SetContactListener(bulletListenerAndFilter);
            pWorld_.SetContactFilter(bulletListenerAndFilter);

            fiber_.Schedule(HandleUpdate, GlobalDefs.TickInterval);
            fiber_.ScheduleOnInterval(SendPlayerStatuses, 500, 500);
            fiber_.ScheduleOnInterval(UpdateMap, 0, 15000);
            fiber_.Schedule(FinishGame, mode.GetMatchDuration());

            fiber_.Start();
        }

        private void HandleUpdate()
        {
            accumulator_ += (CurrentTime.Instance.CurrentTimeInMs - Time);

            while (accumulator_ >= GlobalDefs.TickInterval)  
            {
                accumulator_ -= GlobalDefs.TickInterval;
                Time += GlobalDefs.TickInterval;
                Update();
            }

            fiber_.Schedule(HandleUpdate, GlobalDefs.TickInterval);
        }

        public Map Map
        { get { return map_; } }  

        public int GenerateID()
        {
            return id_++;
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

                //send blocks, mobs, atc
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

        public Bullet SpawnBullet()
        {
            var bullet = new Bullet(); 
            bullet.ID = GenerateID();
            bullet.Game = this;
            bullets_.Add(bullet.ID, bullet);
            return bullet;
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

        public void OnUnitAttack(Unit unit, AttackData attack)
        {
            proto_game.UnitAttack attackData = new proto_game.UnitAttack();
            attackData.direction = attack.Direction;
            attackData.guid = unit.ID;

            var pos = unit.Position;
            attackData.x = pos.x;
            attackData.y = pos.y;

            Broadcast(proto_common.Events.UNIT_ATTACK, attackData);
        }

        public void PlayerTurned(Player player, proto_game.PlayerTurn turnData)
        {
            Broadcast(proto_common.Events.PLAYER_TURN, turnData, player);
        }

        public void PlayerCast(Player player, proto_game.CastSkill.Request castReq)
        {
            var skillCastedEvt = new proto_game.SkillCasted();
            skillCastedEvt.guid = player.ID;
            skillCastedEvt.direction = castReq.direction;
            skillCastedEvt.x = castReq.x;
            skillCastedEvt.y = castReq.y;
            Broadcast(proto_common.Events.SKILL_CASTED, skillCastedEvt, player);
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
                else if (target is ExpBlock)
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
                if (System.Math.Abs(player.HP - player.Stats.GetFinValue(proto_game.Stats.MaxHealth)) > 0.001f) //make fields as classes to utilize message packing
                    Broadcast(proto_common.Events.PLAYER_STATUS_CHANGED, player.GetStatusChanged());
            }
        }

        private void Update()
        {
            long ldt = GlobalDefs.TickInterval;
            float dt = GlobalDefs.GetUpdateInterval();

            var playerMoveResponse = new proto_game.PlayerInput.Response();
            foreach (var plr in joinedPlayers_)
            {
                plr.ProcessInput(dt);  
            }

            pWorld_.Step(dt, 4, 4);
            ProcessDeathList();

            foreach (var plr in joinedPlayers_) 
            {
                if (plr.Input == null) continue;
                //send sync packet
                playerMoveResponse.tick = plr.Input.tick;
                playerMoveResponse.force_x = plr.Input.force_x;
                playerMoveResponse.force_y = plr.Input.force_y;
                playerMoveResponse.x = plr.Body.GetPosition().X;
                playerMoveResponse.y = plr.Body.GetPosition().Y;

                plr.Controller.SendResponse(proto_common.Commands.PLAYER_INPUT, playerMoveResponse);
            }

            var unitMoveEvent = new proto_game.UnitMove();
            foreach (var pair in mobs_)
            {
                continue;
                var mob = pair.Value;
                mob.Update(dt);

                if (mob.Moved)
                {
                    var pos = mob.Position;
                    unitMoveEvent.guid = mob.ID;
                    unitMoveEvent.x = pos.x;
                    unitMoveEvent.y = pos.y;
                    unitMoveEvent.timestamp = Time;
                    unitMoveEvent.tick = Tick; 

                    Broadcast(proto_common.Events.UNIT_MOVE, unitMoveEvent); 
                }
            }

            List<PowerUp> toRemove = ListPool<PowerUp>.Get();
            foreach (var pwr in powerUps_)
            {
                if (pwr.Holder != null)
                {
                    pwr.Lifetime -= (int)ldt;
                    if (pwr.Lifetime <= 0)
                    {
                        //remove power up
                        toRemove.Add(pwr);
                    }
                }
            }
            
            foreach (var pwr in toRemove)
            {
                powerUps_.Remove(pwr);
            }
            ListPool<PowerUp>.Release(toRemove);

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
            Broadcast(proto_common.Events.POWER_UP_APPEARED, powerUp.GetPowerUpPacket());
        }

        public bool TryGrabPowerUp(int powerUpId, Player player)
        {
            bool result = false;
            foreach (var pwr in powerUps_)
            {
                if (pwr.ID == powerUpId && pwr.Holder == null)
                {
                    result = true;
                    pwr.Holder = player;  

                    var grabEvt = new proto_game.PowerUpGrabbed();
                    grabEvt.who_grabbed = player.ID;
                    grabEvt.id = powerUpId;
                    Broadcast(proto_common.Events.POWER_UP_GRABBED, grabEvt, player);
                    break;
                }
            }
            return result;
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
