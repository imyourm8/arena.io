using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Skills
{
    class Skill
    {
        public static Skill Create(proto_game.Skills skillId)
        {
            Skill skill = null;
            if (skillId == proto_game.Skills.BigCannon)
            {
                skill = new GunnerSkill();
            }
            skill.Entry = Factories.SkillFactory.Instance.GetEntry(skillId);
            return skill;
        }

        public Unit Owner
        { get; set; }

        public SkillEntry Entry
        { get; set; }

        public bool Cast()
        {
            OnCast();

            return true;
        }

        protected virtual void OnCast()
        {
        }
    }
}
