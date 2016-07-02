using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    public enum PreparationResult
    {
        OK,
        NOT_ENOUGH_ENERGY,
        NOT_READY
    }

    public enum SpellTargetFilter
    {
        ENEMY,
        FRIENDLY,
        ALL,
        CASTER
    }

    public sealed class StringToSpellTargetFilter
    {
        public static SpellTargetFilter Convert(string str)
        {
            SpellTargetFilter filter;
            switch (str)
            {
                case "enemy":
                    filter = SpellTargetFilter.ENEMY;
                    break;
                case "friendly":
                    filter = SpellTargetFilter.FRIENDLY;
                    break;
                case "all":
                    filter = SpellTargetFilter.ALL;
                    break;
                case "caster":
                    filter = SpellTargetFilter.CASTER;
                    break;
                default:
                    throw new Exception("Unknown Spell target filtering " + str);
            }
            return filter;
        }
    }

    public enum SpellIDs
    {
        AUTO_ATTACK_MELEE,
        WARM_DAMAGE,
        WARM_DEFENSE,
        WARM_SPELL_POWER,
        FIREBALL_EXPLOSION
    }

    public sealed class StringToSpellID
    {
        public static SpellIDs Convert(string str)
        {
            SpellIDs id;
            switch (str)
            {
                case "auto_attack_melee":
                    id = SpellIDs.AUTO_ATTACK_MELEE;
                    break;
                case "warm_damage":
                    id = SpellIDs.WARM_DAMAGE;
                    break;
                case "warm_defense":
                    id = SpellIDs.WARM_DEFENSE;
                    break;
                case "warm_spell_power":
                    id = SpellIDs.WARM_SPELL_POWER;
                    break;
                case "fireball_explosion":
                    id = SpellIDs.FIREBALL_EXPLOSION;
                    break;
                default:
                    throw new Exception("Unknown Spell ID "+str);
            }
            return id;
        }
    }
}
