using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class ExpBlock : Entity
    {
        public ExpBlock()
        {
            Category = PhysicsDefs.Category.EXP_BLOCK;
            ReplicateOnClients = true;
            additionalCollisionMask_ = (ushort)PhysicsDefs.Category.BULLET;
        }

        public proto_game.ExpBlocks BlockType
        { get; set; }

        public int Coins
        { get; set; }

        public proto_game.BlockAppeared GetAppearedPacket()
        {
            proto_game.BlockAppeared packet = new proto_game.BlockAppeared();
            packet.guid = ID;
            packet.position = new proto_game.Vector();

            var pos = Position;
            packet.position.x = pos.x;
            packet.position.y = pos.y;
            packet.hp = HP;
            packet.max_hp = Stats.GetFinValue(proto_game.Stats.MaxHealth);
            packet.type = BlockType;
            return packet;
        }

        public override void InitPhysics()
        {
            base.InitPhysics();
        }
    }
}
