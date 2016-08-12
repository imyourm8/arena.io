using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using arena.battle.Skills;

namespace arena.Factories
{
    class SkillFactory : TapCommon.Singleton<SkillFactory>
    {
        private Dictionary<proto_game.Skills, SkillEntry> skills_ = new Dictionary<proto_game.Skills, SkillEntry>();

        public void Init()
        {
            var jsonSkills = JArray.Parse(File.ReadAllText("game_data/skills_export.json"));

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
