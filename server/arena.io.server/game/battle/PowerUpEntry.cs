using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.battle
{
    class PowerUpEntry
    {
        public PowerUpEntry(JToken data)
        {
            var type = data.SelectToken("type").Value<string>();
            Type = helpers.Parsing.ParseEnum<proto_game.PowerUpType>(type);
            PickUpType = helpers.Parsing.ParseEnum<proto_game.Pickups>(data.SelectToken("pickup_type").Value<string>());
            CollisionRadius = data.SelectToken("radius").Value<float>();
        }

        public proto_game.Pickups PickUpType
        { get; private set; }

        public proto_game.PowerUpType Type
        { get; private set; }

        public float CollisionRadius
        { get; private set; }
    }
}
