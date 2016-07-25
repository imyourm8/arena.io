using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PowerUpSpawnPoint
    {
        private Dictionary<proto_game.PowerUpType, float> probabilities_;
        public Dictionary<proto_game.PowerUpType, float> Probabilities
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

        public helpers.Area Area
        { get; set; }

        public Dictionary<proto_game.PowerUpType, int> Durations
        { get; set; }

        public float TotalWeight
        { get; set; }
    }
}
