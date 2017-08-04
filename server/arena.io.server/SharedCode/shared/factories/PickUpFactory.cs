using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using shared.data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace shared.factories
{
    public class PickUpFactory : Singleton<PickUpFactory>
    {
        private Dictionary<proto_game.Pickups, PickUpEntry> pickUps_ = new Dictionary<proto_game.Pickups, PickUpEntry>();

        public void Init(string dataDirectory)
        {
            var jsonPickups = JArray.Parse(File.ReadAllText(Path.Combine(dataDirectory, "game_data/pickups_export.json")));
            foreach (var pu in jsonPickups)
            {
                var entry = new PickUpEntry(pu);
                pickUps_.Add(entry.Type, entry);
            }
        }

        public PickUpEntry GetEntry(proto_game.Pickups type)
        {
            return pickUps_[type];
        }
    }
}
