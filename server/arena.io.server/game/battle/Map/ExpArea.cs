using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

namespace arena.battle
{
    class ExpArea
    {
        private Dictionary<proto_game.ExpBlocks, float> probabilities_;
        public Dictionary<proto_game.ExpBlocks, float> Probabilities
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

        public TriangulatedPolygon Area
        { get; set; }

        public int Priority
        { get; set; }

        public float TotalWeight
        { get; private set; }
    }
}
