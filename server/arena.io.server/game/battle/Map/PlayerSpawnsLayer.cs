using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle
{
    class PlayerSpawnsLayer
    {
        private List<PlayerSpawnPoint> spawnPoints_;

        public PlayerSpawnsLayer(List<PlayerSpawnPoint> points)
        {
            spawnPoints_ = points;
        }

        public Tuple<float, float> GetSpawnPoint()
        {
            var point = spawnPoints_[MathHelper.Range(0, spawnPoints_.Count - 1)];
            float x, y;
            point.Area.RandomPoint(out x, out y);

            return new Tuple<float, float>(x, y);
        }
    }
}
