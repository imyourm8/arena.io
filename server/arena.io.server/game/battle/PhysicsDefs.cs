using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    internal sealed class PhysicsDefs
    {
        public enum Category : ushort
        {
            MOB = 1,
            PLAYER = 1<<1,
            BULLET = 1<<2,
            WALLS = 1<<3,
            EXP_BLOCK = 1<<4,
            PICKUPS = 1<<5
        }
    }
}
