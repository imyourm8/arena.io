using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PowerUp : PickUp
    {
        public PowerUp()
        {
            additionalCollisionMask_ = (ushort)PhysicsDefs.Category.PLAYER;
        }

        public proto_game.PowerUpType Type
        { get; set; }

        public int Lifetime
        { get; set; }

        public Player Holder
        { get; set; }

        public override Net.EventPacket GetAppearedPacket()
        {
            var powerUpPacket = new proto_game.PowerUpAppeared();
            powerUpPacket.id = ID;

            var pos = Position;
            powerUpPacket.x = pos.x;
            powerUpPacket.y = pos.y;
            powerUpPacket.type = Type;
            powerUpPacket.lifetime = Lifetime;

            return ConstructPacket(proto_common.Events.POWER_UP_APPEARED, powerUpPacket);
        }

        public override void InitPhysics()
        {
            sensorBody_ = true;
            base.InitPhysics();
        }

        public override void OnPickUpBy(Player player)
        {
            Game.TryGrabPowerUp(ID, player);
        }
    }
}
