﻿using System;
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

        public Player Holder
        { get; set; }

        public proto_game.PowerUpAppeared GetPowerUpPacket()
        {
            var powerUpPacket = new proto_game.PowerUpAppeared();
            powerUpPacket.id = ID;

            var pos = Position;
            powerUpPacket.x = pos.x;
            powerUpPacket.y = pos.y;
            powerUpPacket.type = Type;
            powerUpPacket.lifetime = Lifetime;

            return powerUpPacket;
        }
    }
}
