using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.Factories
{
    class BulletFactory : TapCommon.Singleton<BulletFactory>
    {
        private Dictionary<proto_game.Bullets, BulletEntry> bullets_ = new Dictionary<proto_game.Bullets, BulletEntry>();

        public void Init()
        {
            var jsonBullets = JArray.Parse(File.ReadAllText("game_data/bullets_export.json"));

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
