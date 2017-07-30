using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle
{
    struct AttackData
    {
        public float Direction
        { get; set; }

        public Vector2 Position
        { get; set; }

        public int FirstBulletID
        { get; set; }
    }
}
