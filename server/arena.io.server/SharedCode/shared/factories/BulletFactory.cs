using System;
using System.IO;
using System.Collections.Generic;

using shared.data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace shared.factories
{
    public class BulletFactory : Singleton<BulletFactory>
    {
        private Dictionary<proto_game.Bullets, BulletEntry> bullets_ = new Dictionary<proto_game.Bullets, BulletEntry>();

        public void Init(string dataDirectory)
        {
            var jsonBullets = JArray.Parse(File.ReadAllText(Path.Combine(dataDirectory, "game_data/bullets_export.json")));

            foreach (var bulData in jsonBullets)
            {
                var entry = new BulletEntry(bulData);
                bullets_.Add(entry.Type, entry);
            }
        }

        public BulletEntry GetEntry(proto_game.Bullets type)
        {
            return bullets_[type];
        }
    }
}
