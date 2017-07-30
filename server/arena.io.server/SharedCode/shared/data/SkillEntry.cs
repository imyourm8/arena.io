using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace shared.data
{
    public class SkillEntry
    {
        public SkillEntry(JToken data)
        {
            Recoil = data.SelectToken("recoil").Value<float>();
            Type = helpers.Parsing.ParseEnum<proto_game.Skills>(data.SelectToken("type").Value<string>());
        }

        public float Recoil
        { get; private set; }

        public proto_game.Skills Type
        { get; private set; }
    }
}
