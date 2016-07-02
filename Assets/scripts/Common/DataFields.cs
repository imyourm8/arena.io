using System.Collections;
using System.Collections.Generic;

namespace TapCommon
{
    public enum Faction
    {
        LEFT,
        RIGHT
    }

    public enum ObjectType
    { 
        PLAYER = 1,
        DESTRUCTIBLE = 2,
        CREATURE = 4,
        SPELL = 8,
        GROUP = 16
    };

    public enum ObjectFields
    {
        TEMPLATE_ENTRY,
        F_LOCAL_POSITION,
        F_MOVEMENT_SPEED,
        F_OBJ_RADIUS,
        ObjectFields_TOTAL
    };

    public enum UnitFields
    {
        BEGIN_UNUSABLE = ObjectFields.ObjectFields_TOTAL + 1,
        D_HEALTH = BEGIN_UNUSABLE + 1,
        D_DAMAGE_MIN = BEGIN_UNUSABLE + 2,
        L_GOLD = BEGIN_UNUSABLE + 3,
        I_LEVEL = BEGIN_UNUSABLE + 4,
        L_EXP = BEGIN_UNUSABLE + 5,
        F_ATTACK_SPEED = BEGIN_UNUSABLE + 6,
        I_AUTO_ATTACK_SPELL = BEGIN_UNUSABLE + 7,
        F_ATTACK_RADIUS = BEGIN_UNUSABLE + 8,
        D_HEALTH_REGEN = BEGIN_UNUSABLE + 9,
        D_MAX_HEALTH = BEGIN_UNUSABLE + 10,
        F_ENERGY = BEGIN_UNUSABLE + 11,
        D_DAMAGE_MAX = BEGIN_UNUSABLE + 12,
        D_DPS = BEGIN_UNUSABLE + 13,
        UnitFields_TOTAL = BEGIN_UNUSABLE + 15
    };

    public enum CharacterFields 
	{
        BEGIN_UNUSABLE = UnitFields.UnitFields_TOTAL + 1,
        F_DAMAGE_BUMP = BEGIN_UNUSABLE + 1,
        F_DEFENSE_BUMP = BEGIN_UNUSABLE + 2,
        F_SPELLPOWER_BUMP = BEGIN_UNUSABLE + 3,
        PlayerFields_TOTAL = BEGIN_UNUSABLE + 10
	};

    public enum CreatureFields
    {
        BEGIN_UNUSABLE = UnitFields.UnitFields_TOTAL + 1,
        F_AGRO_RADIUS = BEGIN_UNUSABLE + 1,
        I_TYPE = BEGIN_UNUSABLE + 2,
        CreatureFields_TOTAL = BEGIN_UNUSABLE + 5
    };
}
