using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace shared.data
{
    public class PickUpEntry
    {
        public PickUpEntry(JToken data)
        {
            var type = data.SelectToken("type").Value<string>();
            Type = helpers.Parsing.ParseEnum<proto_game.Pickups>(type);
            CollisionRadius = data.SelectToken("radius").Value<float>();
        }

        public proto_game.Pickups Type
        { get; private set; }

        public float CollisionRadius
        { get; private set; }
    }
}
