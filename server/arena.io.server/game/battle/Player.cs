using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Player : Entity, PlayerExperience.IExpProvider
    {
        private HashSet<Entity> visibleEntities_ = new HashSet<Entity>();
        private PlayerExperience exp_;

        public Player(player.PlayerController controller, player.Profile profile)
        {
            Controller = controller;

            Level = 1;
            Profile = profile;
            exp_ = new PlayerExperience(this);
        }

        public player.Profile Profile
        { get; private set; }

        public proto_profile.PlayerClasses SelectedClass
        { get; set; }

        public int Level
        { get; set; }

        public PlayerBattleStats BattleStats
        { get; set; }

        public Room Room
        { get; set; }

        public Game Game
        { get; set; }

        public player.PlayerController Controller
        { get; private set; }

        public void AssignStats()
        {
            var entry = Factories.PlayerClassFactory.Instance.GetEntry(SelectedClass);

            Stats.SetValue(proto_game.Stats.MaxHealth, entry.Health).SetStep(entry.HealthStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.BulletDamage, entry.BulletDamage).SetStep(entry.BulletDamageStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.BulletSpeed, entry.BulletSpeed).SetStep(entry.BulletSpeedStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.HealthRegen, entry.HealthRegen).SetStep(entry.HealthRegenStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.MovementSpeed, entry.MovementSpeed).SetStep(entry.MovementSpeedStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.ReloadSpeed, entry.ReloadSpeed).SetStep(entry.ReloadSpeedStep * -1.0f).ResetSteps();
            Stats.SetValue(proto_game.Stats.SkillDamage, entry.SkillDamage).SetStep(entry.SkillDamageStep).ResetSteps();
            Stats.SetValue(proto_game.Stats.Armor, entry.Armor).SetStep(entry.ArmorStep).ResetSteps();

            HP = Stats.GetFinValue(proto_game.Stats.MaxHealth);
        }

        public proto_game.PlayerAppeared GetAppearedPacket()
        {
            var appearData = new proto_game.PlayerAppeared();

            appearData.name = Profile.Name;
            appearData.guid = ID;
            appearData.hp = HP;
            appearData.stats = GetStatsPacket();
            appearData.position = new proto_game.Vector();
            appearData.position.x = X;
            appearData.position.y = Y;
            appearData.@class = SelectedClass;

            return appearData;
        }

        public proto_game.PlayerDisconnected GetDisconnectedPacket()
        {
            var disconnectPacket = new proto_game.PlayerDisconnected();
            disconnectPacket.who = ID;
            return disconnectPacket;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            HP = Math.Min(HP + Stats.GetFinValue(proto_game.Stats.HealthRegen) * dt, Stats.GetFinValue(proto_game.Stats.MaxHealth));
        }

        public proto_game.PlayerStatusChange GetStatusChanged()
        {
            proto_game.PlayerStatusChange statusPacket = new proto_game.PlayerStatusChange();
            statusPacket.guid = ID;
            statusPacket.hp = HP;
            return statusPacket;
        }

        public void AddExperience(int value)
        {
            exp_.AddExperience(value);
        }
    }
}
