using System.Data;

using shared.helpers;

namespace shared.data
{
    public class BoosterEntry
    {
        public BoosterEntry(IDataReader data)
        {
            Type = Parsing.ParseEnum<proto_game.BoosterType>((string)data["type"]);
        }

        public proto_game.BoosterType Type { get; private set; }
    }
}
