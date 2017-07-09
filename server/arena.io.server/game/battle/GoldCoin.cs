using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class GoldCoin : PickUp
    {
        public int Value
        { get; set; }

        public override void OnPickUpBy(Player player)
        {
            Game.OnCoinPickedUp(player, this);
        }
    }
}
