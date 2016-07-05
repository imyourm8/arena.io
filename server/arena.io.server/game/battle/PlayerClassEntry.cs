using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace arena.battle
{
    class PlayerClassEntry
    {
        public PlayerClassEntry(IDataReader data)
        {
            if (!data.Read())
            {
                return;
            }

            @Class = helpers.Parsing.ParseEnum<proto_profile.PlayerClasses>((string)data["class"]);
            Price = (int)data["price"];
            MinLevel = (int)data["level_required"];
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
        }

        public proto_profile.PlayerClasses @Class
        { get; private set; }

        public int Price
        { get; private set; }

        public int MinLevel
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
    }
}
