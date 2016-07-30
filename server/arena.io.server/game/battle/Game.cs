using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ExitGames.Concurrency.Fibers;

using arena.helpers;

namespace arena.battle
{
    class Game : player.IActionInvoker, BlockSpawner.IBlockControl
    {
        private HashSet<Player> joinedPlayers_ = new HashSet<Player>();
        private Dictionary<int, Entity> units_ = new Dictionary<int, Entity>();//use concurrent dictionary
        private List<Player> players_ = new List<Player>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private PoolFiber fiber_ = new PoolFiber();
        private int id_ = 0;
        private long lastUpdateTime_;
        private GameModes.GameMode mode_;
        private long matchFinishAt_;
        private Room room_;
        private Map map_;

        public Game(GameModes.GameMode mode, Room room)    
        {
            room_ = room;
            fiber_.ScheduleOnInterval(Update, 200, 200);
            fiber_.ScheduleOnInterval(SendPlayerStatuses, 500, 500);
            fiber_.ScheduleOnInterval(UpdateSpawner, 0, 15000);
            fiber_.Schedule(FinishGame, mode.GetMatchDuration());
            matchFinishAt_ = CurrentTime.Instance.CurrentTimeInMs + mode.GetMatchDuration();

            lastUpdateTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;

            mode_ = mode;
            mode_.Game = this; 

            var mapLoader = new MapLoader(mode_.GetMapPath(), this);    
            map_ = mapLoader.Load(); 

            fiber_.Start();
        }

        public Map Map
        { get { return map_; } }  

        public int GenerateID()
        {
            return id_++;
        }

        private void UpdateSpawner()
        {
            map_.Update();
        }

        public void Add(Player player)
        {
            fiber_.Enqueue(() =>
            {
                player.Game = this;

                player.BattleStats.Reset();

                //add to players pool
                players_.Add(player);
            });
        }

        public void PlayerJoin(Player player)
        {
            fiber_.Enqueue(() =>
            {
                player.ID = GenerateID();
                mode_.SpawnPlayer(player);
                map_.Add(player);
                player.AssignStats();
    
                //send join game packet
                var joinPacket = new proto_game.JoinGame.Response();
                var outerBorder = map_.GetOuterBorder();
                foreach (var coord in outerBorder)
                {
                    joinPacket.outer_border.Add(coord); 
                }
                joinPacket.time_left = (int)(matchFinishAt_ - CurrentTime.Instance.CurrentTimeInMs);
                player.Controller.SendResponse(proto_common.Commands.JOIN_GAME, joinPacket); 

                //broadcast new player across other players
                var appearedPacket = player.GetAppearedPacket(); 
                Broadcast(proto_common.Events.PLAYER_APPEARED, appearedPacket); 

                //send packet to connected player aswell
                appearedPacket.local = true;
                player.Controller.SendEvent(proto_common.Events.PLAYER_APPEARED, appearedPacket);

                //now gather all online players now and send to new player
                foreach (var p in joinedPlayers_)
                {
                    player.Controller.SendEvent(proto_common.Events.PLAYER_APPEARED, p.GetAppearedPacket());
                }

                //send blocks
                foreach (var e in units_)
                {
                    var block = e.Value as ExpBlock;
                    if (block == null) continue;
                    player.Controller.SendEvent(proto_common.Events.BLOCK_APPEARED, block.GetBlockAppearPacket());
                }

                Add((Entity)player);
                joinedPlayers_.Add(player);
            });
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

        public void PlayerMoved(Player player, proto_game.PlayerMove.Request movePacket)
        {
            var req = new proto_game.UnitMove();

            req.x = movePacket.position.x;
            req.y = movePacket.position.y;
            req.timestamp = movePacket.timestamp;
            req.guid = player.ID;
            req.stop = movePacket.stop;

            var oldX = player.X;
            var oldY = player.Y;

            map_.Move(player, req.x, req.y);
            req.x = player.X;
            req.y = player.Y;

            var dx = player.X - oldX;
            var dy = player.Y - oldY;
            player.BattleStats.DistanceTraveled +=
                (int)(dx*dx + dy*dy);

            Broadcast(proto_common.Events.UNIT_MOVE, req, player);
        }

        public void PlayerAttacked(Player player, proto_game.UnitAttack attackData)
        {
            Broadcast(proto_common.Events.UNIT_ATTACK, attackData, player);
        }

        public void PlayerTurned(Player player, proto_game.PlayerTurn turnData)
        {
            Broadcast(proto_common.Events.PLAYER_TURN, turnData, player);
        }

        public void UnitDamaged(Player player, proto_game.ApplyDamage dmgData)
        {
            var target = GetEntity(dmgData.target);
            if (target != null)
            {
                target.HP -= dmgData.damage;

                var dmgDonePacket = new proto_game.DamageDone();
                dmgDonePacket.hp_left = target.HP;
                dmgDonePacket.target = target.ID;

                Broadcast(proto_common.Events.DAMAGE_DONE, dmgDonePacket);

                if (target.HP < 0.00001f)
                {
                    mode_.HandleEntityKill(player, target);
                    Broadcast(proto_common.Events.UNIT_DIE, target.GetDiePacket());

                    int expGenerated = target.Exp;
                    int score = 0;
                    int gold = 0;

                    if (target is Player)
                    {
                        var targetPlayer = target as Player;
                        PlayerDead(targetPlayer);

                        var lvlDiff = Math.Max(1, targetPlayer.Level - player.Level);
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

                    player.BattleStats.Score += score;
                    player.BattleStats.Gold += gold;

                    var expPacket = new proto_game.PlayerExperience();
                    expPacket.exp = expGenerated;
                    player.AddExperience(expGenerated);
                    player.Controller.SendEvent(proto_common.Events.PLAYER_EXPERIENCE, expPacket);
                }
            }
        }

        private void SpawnGoldPiles(ExpBlock block)
        {
            
        }

        private void Remove(Entity mob)
        {
            units_.Remove(mob.ID);
            map_.Remove(mob);
        }

        private void Add(Entity unit)
        {
            units_.Add(unit.ID, unit);
            map_.Add(unit);
        }

        void BlockSpawner.IBlockControl.AddBlock(ExpBlock block)
        {
            block.ID = GenerateID();
            Add(block); 
            Broadcast(proto_common.Events.BLOCK_APPEARED, block.GetBlockAppearPacket());
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
                if (Math.Abs(player.HP - player.Stats.GetFinValue(proto_game.Stats.MaxHealth)) > 0.001f) //make fields as classes to utilize message packing
                    Broadcast(proto_common.Events.PLAYER_STATUS_CHANGED, player.GetStatusChanged());
            }
        }

        private void Update()
        {
            long ldt = helpers.CurrentTime.Instance.CurrentTimeInMs - lastUpdateTime_;
            float dt = (float)ldt;
            dt /= 1000.0f;


            foreach (var unit in units_)
            {
                unit.Value.Update(dt);
            }

            List<PowerUp> toRemove = new List<PowerUp>(4);
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

            mode_.Update(dt);

            lastUpdateTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
        }

        private void Broadcast(proto_common.Events evtKey, object msg, Player exceptThis = null)
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
