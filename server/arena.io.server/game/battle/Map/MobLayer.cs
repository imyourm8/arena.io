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
            int i = 0;
            prevSpawnTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
            foreach (var spawn in spawnPoints_)
            {
                while (spawn.Count < spawn.MaxCount) 
                {
                    var type = helpers.Extensions.PickRandom<proto_game.MobType>(spawn.Probabilities, spawn.TotalWeight);
                    Mob mob = Mob.Create(type);
                    float x, y;
                    spawn.Area.RandomPoint(out x, out y);
                    Game.Add(mob);
                    //save to track spawn spot
                    mob.SpawnPoint = spawn;
                    mob.SetPosition(x, y);
                    spawn.Count++;
                    break;
                }
                i += spawn.Count;
                if (i == 1) break;
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
