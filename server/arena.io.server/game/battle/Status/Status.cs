using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Status
{
    class Status : arena.common.battle.Status
    {
        public Status(proto_game.PowerUpType t, float removeAfter)
            : base(t, removeAfter)
        {
 
        }
    }
}
