using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PowerUp : Entity
    {
        public proto_game.PowerUpType Type
        { get; set; }

        public int Lifetime
        { get; set; }

        public bool Holded
        { get; set; }
    }
}
