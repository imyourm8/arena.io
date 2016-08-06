using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.battle
{
    class WeaponEntry
    {
        public class SpawnPoint
        {
            public SpawnPoint(JToken data)
            {
                Position = new Vector2(data["x"].Value<float>(), data["y"].Value<float>());
                Rotation = data["rot"].Value<float>();
                Bullet = helpers.Parsing.ParseEnum<proto_game.Bullets>(data["bullet"].Value<string>());
            }

            public Vector2 Position
            { get; private set; }

            public float Rotation
            { get; private set; }

            public proto_game.Bullets Bullet
            { get; private set; }
        }

        private List<SpawnPoint> spawnPoints_ = new List<SpawnPoint>();

        public WeaponEntry(JToken data)
        {
            foreach (var spawnPointData in data["sp"])
            {
                var spawnPoint = new SpawnPoint(spawnPointData);
                spawnPoints_.Add(spawnPoint);
            }

            Type = helpers.Parsing.ParseEnum<proto_game.Weapons>(data["type"].Value<string>());
        }

        public IReadOnlyList<SpawnPoint> SpawnPoints
        { get { return spawnPoints_; } }

        public proto_game.Weapons Type
        { get; set; }
    }
}
