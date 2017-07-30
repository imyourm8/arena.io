using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.helpers
{
    static class Extensions
    {
        public static T PickRandom<T>(Dictionary<T, float> probabilities, float totalWeight)
        {
            float blockWeight = totalWeight;
            float blockRoll = MathHelper.Range(0, blockWeight);
            float totalRoll = 0;
            T type = default(T);
            foreach (var blockSpawn in probabilities)
            {
                if (blockRoll > totalRoll)
                {
                    type = blockSpawn.Key;
                }

                if (totalRoll > blockRoll) break;

                totalRoll += blockSpawn.Value;
            }
            return type;
        }
    }
}
