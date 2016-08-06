using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2DX.Common;
using Box2DX.Dynamics;

using Nito;

namespace arena.battle
{
    class Player : Unit, PlayerExperience.IExpProvider
    {
        private static readonly int MaxStoredInputs = 100;

        private HashSet<Entity> visibleEntities_ = new HashSet<Entity>();
        private PlayerExperience exp_;
        private Deque<proto_game.PlayerInput.Request> inputs_ = new Deque<proto_game.PlayerInput.Request>();

        public Player(player.PlayerController controller, player.Profile profile)
        {
            Controller = controller;

            Category = PhysicsDefs.Category.PLAYER;
            Level = 1;
            Profile = profile;
            exp_ = new PlayerExperience(this);
            BattleStats = new PlayerBattleStats();
        }

        public proto_game.PlayerInput.Request Input
        { get; set; }

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
            Stats.SetValue(proto_game.Stats.SkillCooldown, entry.SkillCooldown).SetStep(0).ResetSteps();
            Stats.SetValue(proto_game.Stats.Armor, entry.Armor).SetStep(entry.ArmorStep).ResetSteps();

            HP = Stats.GetFinValue(proto_game.Stats.MaxHealth); 
            Skill = entry.SKill;
            Radius = entry.CollisionRadius;
            Weapon = Factories.WeaponFactory.Instance.GetEntry(entry.Weapon);
        }

        public proto_game.PlayerAppeared GetAppearedPacket()
        {
            var appearData = new proto_game.PlayerAppeared(); 

            appearData.name = Profile.Name;
            appearData.guid = ID;
            appearData.hp = HP;
            appearData.level = Level;
            appearData.stats = GetStatsPacket();
            appearData.position = new proto_game.Vector();

            var pos = Position;
            appearData.position.x = pos.x;
            appearData.position.y = pos.y;
            appearData.@class = SelectedClass;
            appearData.skill = Skill;

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
            HP = System.Math.Min(HP + Stats.GetFinValue(proto_game.Stats.HealthRegen) * dt, Stats.GetFinValue(proto_game.Stats.MaxHealth));
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

        public void ProcessInput(float dt)
        { 
            if (inputs_.Count == 0) return;

            var input = Input;
            if (input != null)
            {
                MoveInDirection(new helpers.Vector2(input.force_x, input.force_y));
                Rotation = input.rotation;

                if (input.shoot != null)
                {
                    PerformAttackAtDirection(input.shoot.direction);
                }
            }

            //process only one tick at time
            input = null;
            int removeUntillElement = 0;
            while (inputs_.Count != removeUntillElement)
            {
                input = inputs_[removeUntillElement++];
                if (!IsInputValid(input))
                {
                    input = null;
                }
                else
                {
                    break;
                }
            }

            Input = input;

            if (removeUntillElement > 0)
            {
                inputs_.RemoveRange(0, removeUntillElement);
            }
        }

        public void AddInput(proto_game.PlayerInput.Request input)
        {
            if (inputs_.Count == MaxStoredInputs) 
                inputs_.RemoveFromFront();

            inputs_.AddToBack(input);
        }

        private bool IsInputValid(proto_game.PlayerInput.Request input)
        {
            int maxTickDelta = (int)( 1.0f / GlobalDefs.GetUpdateInterval() * 2.5f ) ;
	        int nMinDelta = System.Math.Max( 0, Game.Tick - maxTickDelta );
	        int nMaxDelta = Game.Tick + maxTickDelta;

            bool valid = (input.tick >= nMinDelta && input.tick < nMaxDelta);

            return valid || true;
        }
    }
}
