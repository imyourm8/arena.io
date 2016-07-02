using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    public enum StatusEffects
    { 
        NONE,
        WARM_DAMAGE,
        WARM_DEFENSE,
        WARM_SPELLPOWER
    }

    public sealed class StringToStatusEffect
    {
        public static StatusEffects Convert(string str)
        {
            StatusEffects status;
            switch (str)
            {
                case "damage_warm":
                    status = StatusEffects.WARM_DAMAGE;
                    break;
                case "defense_warm":
                    status = StatusEffects.WARM_DEFENSE;
                    break;
                case "spell_power_warm":
                    status = StatusEffects.WARM_SPELLPOWER;
                    break;
                case "none":
                    status = StatusEffects.NONE;
                    break;
                default:
                    throw new Exception("Unknown Status Effect " + str);
            }
            return status;
        }
    }
}
