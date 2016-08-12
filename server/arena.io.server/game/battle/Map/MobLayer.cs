using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class MobLayer
    {
        private List<MobSpawnPoint> spawnPoints_;
        private long prevSpawnTime_ = 0;

        public MobLayer(List<MobSpawnPoint> points)
        {
            spawnPoints_ = points;
        }

        public Game Game
        { get; set; }

        public long RespawnDelay
        { get; set; }

        public void Update()
        {
            if (helpers.CurrentTime.Instance.CurrentTimeInMs - prevSpawnTime_ < RespawnDelay)
            {
                return; 
            }
            
            prevSpawnTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
            foreach (var spawn in spawnPoints_)
            {
                while (spawn.Count < spawn.MaxCount)
                {
                    var type = helpers.Extensions.PickRandom<proto_game.MobType>(spawn.Probabilities, spawn.TotalWeight);
                    Mob mob = new Mob();
                    float x, y;
                    spawn.Area.RandomPoint(out x, out y);
                    mob.Game = Game;
                    mob.MobType = type;
                    mob.SpawnPoint = spawn;
                    mob.AssignStats();
                    mob.InitPhysics();
                    mob.SetPosition(x, y);
                    spawn.Count++;

                    MobAI.BaseAI ai = null;
                    switch (mob.Entry.AI)
                    {
                        case MobAI.TypesOfAI.Chasing:
                            ai = new MobAI.ChasingAI();
                            break;
                    }
                    mob.AI = ai;
                    Game.Add(mob);
                }
            }
        }

        public void OnMobDead(Mob mob)
        {
            if (mob.SpawnPoint != null)
            {
                mob.SpawnPoint.Count--;
                mob.SpawnPoint = null;
                Game.Remove(mob);
            }
        }
    }
}
