using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.map
{
    static class MapIDs
    {
        public static readonly string FFA_1 = "maps/ffa";

        public static readonly Dictionary<string, int> MapNameToID = new Dictionary<string, int>
        {
            { FFA_1, 0 }
        };
    };
}
