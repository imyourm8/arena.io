using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace shared.data
{
    public class ExpBlockEntry
    {
        public ExpBlockEntry(IDataReader data)
        {
            Exp = (int)data["exp"];
            Gold = (int)data["gold"];
            Health = (float)data["health"];
            Type = helpers.Parsing.ParseEnum<proto_game.ExpBlocks>((string)data["type"]);
        }

        public int Exp
        { get; private set; }

        public int Gold
        { get; private set; }

        public float Health
        { get; private set; }

        public float CollisionRadius
        { get; set; }

        public proto_game.ExpBlocks Type
        { get; private set; }
    }
}
