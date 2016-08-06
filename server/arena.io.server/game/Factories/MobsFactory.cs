using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.Factories
{
    class MobsFactory : TapCommon.Singleton<MobsFactory>
    {
        private Dictionary<proto_game.MobType, battle.MobEntry> entries_ = new Dictionary<proto_game.MobType, battle.MobEntry>();
        public void Init()
        {
            Database.Database.Instance.GetCreatureDB().GetAll(LoadMobs);
        }

        private void LoadMobs(Database.QueryResult result, IDataReader reader)
        {
            if (result != Database.QueryResult.Success)
            {
                return;
            }

            var jsonPlayers = JArray.Parse(File.ReadAllText("game_data/mobs_export.json"));
            var dict = new Dictionary<proto_game.MobType, float>();
            foreach (JToken plr in jsonPlayers)
            {
                var type = helpers.Parsing.ParseEnum<proto_game.MobType>(plr.SelectToken("type").Value<string>());
                var radius = plr.SelectToken("radius").Value<float>();
                dict.Add(type, radius);
            }

            while (reader.Read())
            {
                var entry = new battle.MobEntry(reader);
                entries_.Add(entry.MobType, entry);
                entry.CollisionRadius = dict[entry.MobType];
            }
        }

        public battle.MobEntry GetEntry(proto_game.MobType t)
        {
            return entries_[t];
        }
    }
}
