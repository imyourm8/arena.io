using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PickUp : Entity
    {
        public PickUp()
        {
            Category = PhysicsDefs.Category.PICKUPS;
        }
    }
}
