using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class MobEntry
    {
        public MobEntry(IDataReader data)
        {
            MobType = helpers.Parsing.ParseEnum<proto_game.MobType>((string)data["mob_type"]);
            Health = (float)data["health"];
            HealthStep = (float)data["health_step"];
            HealthRegen = (float)data["health_regen"];
            HealthRegenStep = (float)data["health_regen_step"];
            BulletDamage = (float)data["bullet_damage"];
            BulletDamageStep = (float)data["bullet_damage_step"];
            ReloadSpeed = (float)data["reload_speed"];
            ReloadSpeedStep = (float)data["reload_speed_step"];
            BulletSpeed = (float)data["bullet_speed"];
            BulletSpeedStep = (float)data["bullet_speed_step"];
            MovementSpeed = (float)data["movement_speed"];
            MovementSpeedStep = (float)data["movement_speed_step"];
            Armor = (float)data["armor"];
            ArmorStep = (float)data["armor_step"];
            SkillDamage = (float)data["skill_damage"];
            SkillDamageStep = (float)data["skill_damage_step"];
            Skill = helpers.Parsing.ParseEnum<proto_game.Skills>((string)data["skill"]);
            SkillCooldown = (float)(int)data["skill_cooldown"];
            Exp = (int)data["exp"];
            AgroRange = (float)data["agro_range"];
            ReturnRange = (float)data["return_range"];
            AttackRange = (float)data["attack_range"];
            AI = helpers.Parsing.ParseEnum<MobAI.TypesOfAI>((string)data["ai"]);
            Weapon = helpers.Parsing.ParseEnum<proto_game.Weapons>((string)data["weapon"]);
        }

        public proto_game.MobType MobType
        { get; private set; }

        public float Health
        { get; private set; }

        public float HealthStep
        { get; private set; }

        public float HealthRegen
        { get; private set; }

        public float HealthRegenStep
        { get; private set; }

        public float BulletDamage
        { get; private set; }

        public float BulletDamageStep
        { get; private set; }

        public float BulletSpeed
        { get; private set; }

        public float BulletSpeedStep
        { get; private set; }

        public float MovementSpeed
        { get; private set; }

        public float MovementSpeedStep
        { get; private set; }

        public float Armor
        { get; private set; }

        public float ArmorStep
        { get; private set; }

        public float SkillDamage
        { get; private set; }

        public float SkillDamageStep
        { get; private set; }

        public float ReloadSpeed
        { get; private set; }

        public float ReloadSpeedStep
        { get; private set; }

        public proto_game.Skills Skill
        { get; private set; }

        public float SkillCooldown
        { get; private set; }

        public int Exp
        { get; private set; }

        public float AgroRange
        { get; private set; }

        public float ReturnRange
        { get; private set; }

        public float AttackRange
        { get; private set; }

        public MobAI.TypesOfAI AI
        { get; private set; }

        public proto_game.Weapons Weapon
        { get; private set; }

        public float CollisionRadius
        { get; set; }
    }
}
