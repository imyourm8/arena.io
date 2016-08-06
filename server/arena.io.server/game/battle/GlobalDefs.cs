using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    static class GlobalDefs
    {
        public static readonly int TickInterval = 50;
        public static float GetUpdateInterval()
        {
            return (float)TickInterval / 1000.0f;
        }
    }
}
