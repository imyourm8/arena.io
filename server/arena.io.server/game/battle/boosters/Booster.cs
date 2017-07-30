using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.data;

namespace arena.battle.boosters
{
    class Booster
    {
        public BoosterEntry Entry { get; set; }

        public virtual void Activate()
        { }
    }
}
