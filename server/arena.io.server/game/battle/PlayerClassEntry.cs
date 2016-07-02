using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace arena.battle
{
    class PlayerClassEntry
    {
        public PlayerClassEntry(IDataReader data)
        {
            if (!data.Read())
            {
                return;
            }

            @Class = helpers.Parsing.ParseEnum<proto_profile.PlayerClasses>((string)data["class"]);
            Price = (int)data["price"];
            MinLevel = (int)data["level_required"];
        }

        public proto_profile.PlayerClasses @Class
        { get; private set; }

        public int Price
        { get; private set; }

        public int MinLevel
        { get; private set; }
    }
}
