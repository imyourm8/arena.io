using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using shared.data;

namespace shared.factories
{
    public class SkillFactory : Singleton<SkillFactory>
    {
        private Dictionary<proto_game.Skills, SkillEntry> skills_ = new Dictionary<proto_game.Skills, SkillEntry>();

        public void Init(string dataDirectory)
        {
            var jsonSkills = JArray.Parse(File.ReadAllText(Path.Combine(dataDirectory, "game_data/skills_export.json")));

            foreach (var skillData in jsonSkills)
            {
                var entry = new SkillEntry(skillData); 
                skills_.Add(entry.Type, entry); 
            }
        }

        public SkillEntry GetEntry(proto_game.Skills type)
        {
            return skills_[type];
        }
    }
}
