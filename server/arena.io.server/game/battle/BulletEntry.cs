using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.battle
{
    class BulletEntry
    {
        public BulletEntry(JToken data) 
        {
            TimeAlive = data["time_alive"].Value<float>();
            Radius = data["radius"].Value<float>();
            Speed = data["speed"].Value<float>(); 
            Penetrate = data["penetrate"].Value<bool>();
            Type = helpers.Parsing.ParseEnum<proto_game.Bullets>(data["type"].Value<string>());
        }

        public float TimeAlive
        { get; private set; }

        public float Radius
        { get; private set; }

        public bool Penetrate
        { get; private set; }

        public float Speed
        { get; private set; }

        public proto_game.Bullets Type
        { get; private set; }
    }
}
