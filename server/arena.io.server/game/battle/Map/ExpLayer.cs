using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class ExpLayer
    {
        private List<ExpArea> areas_;

        public ExpLayer(List<ExpArea> areas)
        {
            areas_ = areas;
        }

        public int MaxBlocks
        { get; set; }

        public int TileWidth
        { get; set; }

        public int TileHeight
        { get; set; }

        public helpers.Area Area
        { get; set; }

        public proto_game.ExpBlocks GetBlockTypeByPoint(float x, float y)
        {
            proto_game.ExpBlocks blockType = proto_game.ExpBlocks.Small;
            ExpArea lastArea = null;
            foreach (var area in areas_) 
            {
                if ((lastArea == null || lastArea.Priority < area.Priority) 
                    && area.Area.IsInside(x, y))
                {
                    lastArea = area;
                }
            }

            if (lastArea != null)
            {
                blockType = helpers.Extensions.PickRandom<proto_game.ExpBlocks>(lastArea.Probabilities, lastArea.TotalWeight);
            }

            return blockType;
        }
    }
}
