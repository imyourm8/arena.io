using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class ExpBlock : Entity
    {
        public proto_game.ExpBlocks BlockType
        { get; set; }

        public int Coins
        { get; set; }

        public proto_game.BlockAppeared GetBlockAppearPacket()
        {
            proto_game.BlockAppeared packet = new proto_game.BlockAppeared();
            packet.guid = ID;
            packet.position = new proto_game.Vector();
            packet.position.x = X;
            packet.position.y = Y;
            packet.hp = HP;
            packet.max_hp = Stats.GetFinValue(proto_game.Stats.MaxHealth);
            packet.type = BlockType;
            return packet;
        }
    }
}
