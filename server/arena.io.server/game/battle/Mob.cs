using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Mob : Unit
    {
        public Mob()
        {
            Category = PhysicsDefs.Category.MOB;
        }

        private MobAI.BaseAI ai_;
        public MobAI.BaseAI AI
        {
            set 
            { 
                ai_ = value;
                ai_.Owner = this;
            }
        }

        public proto_game.MobType MobType
        { get; set; }

        public MobEntry Entry
        { get; set; }

        public MobSpawnPoint SpawnPoint
        { get; set; }

        public void AssignStats()
        {
            var entry = Factories.MobsFactory.Instance.GetEntry(MobType);

            Stats.SetValue(proto_game.Stats.MaxHealth, entry.Health).SetStep(entry.HealthStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.BulletDamage, entry.BulletDamage).SetStep(entry.BulletDamageStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.BulletSpeed, entry.BulletSpeed).SetStep(entry.BulletSpeedStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.HealthRegen, entry.HealthRegen).SetStep(entry.HealthRegenStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.MovementSpeed, entry.MovementSpeed).SetStep(entry.MovementSpeedStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.ReloadSpeed, entry.ReloadSpeed).SetStep(entry.ReloadSpeedStep * -1.0f).ResetSteps();
            Stats.SetValue(proto_game.Stats.SkillDamage, entry.SkillDamage).SetStep(entry.SkillDamageStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.Armor, entry.Armor).SetStep(entry.ArmorStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.SkillCooldown, entry.SkillCooldown).SetStep(0).ResetSteps();
            Stats.SetValue(proto_game.Stats.Armor, entry.Armor).SetStep(entry.ArmorStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.BulletSize, entry.BulletSize);

            HP = Stats.GetFinValue(proto_game.Stats.MaxHealth);
            Exp = entry.Exp;
            Skill = Skills.Skill.Create(entry.Skill);
            Radius = entry.CollisionRadius;
            Weapon = Factories.WeaponFactory.Instance.GetEntry(entry.Weapon);
            Entry = entry;
        }

        public proto_game.MobAppeared GetAppearedPacket()
        {
            var appearData = new proto_game.MobAppeared();

            var pos = Position;
            appearData.x = pos.x;
            appearData.y = pos.y;
            appearData.id = ID;
            appearData.hp = HP;
            FillStatsPacket(appearData.stats);
            appearData.type = MobType;
            appearData.weapon_used = Weapon.Type;
            appearData.attack_range = Entry.AttackRange;

            return appearData;
        }

        public override void Update(float dt)
        {
 	        base.Update(dt);
            ai_.Update(dt);
        }
    }
}
