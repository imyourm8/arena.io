using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle
{
    class MobSpawnPoint
    {
        public Area Area
        { get; set; }

        public float TotalWeight
        { get; set; }

        private Dictionary<proto_game.MobType, float> probabilities_;
        public Dictionary<proto_game.MobType, float> Probabilities
        {
            set
            {
                TotalWeight = 0;

                foreach (var prob in value)
                {
                    TotalWeight += prob.Value;
                }
                probabilities_ = value;
            }

            get
            {
                return probabilities_;
            }
        }

        public int Count
        { get; set; }

        public int MaxCount
        { get; set; }
    }
}
