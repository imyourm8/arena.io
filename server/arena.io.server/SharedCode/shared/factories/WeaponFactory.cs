using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace shared.factories
{
    public class WeaponFactory : Singleton<WeaponFactory> 
    {
        private Dictionary<proto_game.Weapons, WeaponEntry> weapons_ = new Dictionary<proto_game.Weapons, WeaponEntry>();

        public void Init(string dataDirectory)  
        {
            var jsonWeapons = JArray.Parse(File.ReadAllText(Path.Combine(dataDirectory, "game_data/weapon_export.json")));

            foreach (JToken wepData in jsonWeapons)
            {
                var entry = new WeaponEntry(wepData);
                weapons_.Add(entry.Type, entry);
            }
        }

        public WeaponEntry GetEntry(proto_game.Weapons type)
        {
            return weapons_[type];
        }
    }
}
