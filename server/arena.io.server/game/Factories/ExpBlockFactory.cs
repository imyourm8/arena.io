using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;

using arena.battle;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.Factories
{
    class ExpBlockFactory : TapCommon.Singleton<ExpBlockFactory>
    {
        private Dictionary<proto_game.ExpBlocks, ExpBlockEntry> blocks_ = new Dictionary<proto_game.ExpBlocks, ExpBlockEntry>();

        public void Init()
        {
            Database.Database.Instance.GetWorldDB().GetExpBlocks(LoadExpBlocks);
        }

        private void LoadExpBlocks(Database.QueryResult result, IDataReader reader)
        {
            var jsonBlocks = JArray.Parse(File.ReadAllText("game_data/exp_blocks_export.json"));
            var dict = new Dictionary<proto_game.ExpBlocks, float>();
            foreach (JToken plr in jsonBlocks)
            {
                var type = helpers.Parsing.ParseEnum<proto_game.ExpBlocks>(plr.SelectToken("type").Value<string>());
                var radius = plr.SelectToken("radius").Value<float>();
                dict.Add(type, radius); 
            }

            while (reader.Read())
            {
                var entry = new ExpBlockEntry(reader);
                entry.CollisionRadius = dict[entry.Type];
                blocks_.Add(entry.Type, entry);
            }
        }

        public ExpBlockEntry GetEntry(proto_game.ExpBlocks type)
        {
            return blocks_[type];
        }
    }
}
