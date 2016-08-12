using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using arena.battle;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.Factories
{
    class PlayerClassFactory : TapCommon.Singleton<PlayerClassFactory>
    {
        private Dictionary<proto_profile.PlayerClasses, PlayerClassEntry> classes_ = new Dictionary<proto_profile.PlayerClasses, PlayerClassEntry>();

        public void Init()
        {
            Database.Database.Instance.GetPLayerDB().GetClasses(HandleClassesFromDB);
        }

        private void HandleClassesFromDB(Database.QueryResult result, IDataReader data)
        {
            if (result != Database.QueryResult.Success)
            {
                return;
            }

            var jsonPlayers = JArray.Parse(File.ReadAllText("game_data/players_export.json"));
            var dict = new Dictionary<proto_profile.PlayerClasses, JToken>();
            foreach (JToken plr in jsonPlayers)
            {
                var @class = helpers.Parsing.ParseEnum<proto_profile.PlayerClasses>(plr.SelectToken("class").Value<string>());
                dict.Add(@class, plr);
            }

            while(data.Read())
            {
                var entry = new PlayerClassEntry(data); 
                classes_.Add(entry.@Class, entry);

                var plr = dict[entry.@Class];
                entry.CollisionRadius = plr.SelectToken("radius").Value<float>();
                entry.LinearDumping = plr.SelectToken("linear_dumping").Value<float>();
                entry.Weapon = helpers.Parsing.ParseEnum<proto_game.Weapons>(plr.SelectToken("weapon").Value<string>());
            };
        }

        public PlayerClassEntry GetEntry(proto_profile.PlayerClasses cls)
        {
            return classes_[cls];
        }

        public IReadOnlyDictionary<proto_profile.PlayerClasses, PlayerClassEntry> GetAllClasses()
        {
            return classes_;
        }
    }
}
