using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2DX.Common;
using Box2DX.Dynamics;

using Nito;

using arena.common.battle;

namespace arena.battle
{
    class Player : Unit, PlayerExperience.IExpProvider
    {
        private static readonly int MaxStoredInputs = 100;

        private HashSet<Entity> visibleEntities_ = new HashSet<Entity>();
        private PlayerExperience exp_;
        private Deque<proto_game.PlayerInput.Request> inputs_ = new Deque<proto_game.PlayerInput.Request>();
        private StatusManager statusManager_;

        public Player(player.PlayerController controller, player.Profile profile)
        {
            Controller = controller;
            statusManager_ = new StatusManager(this); 
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
            AddSkill(entry.Skill);
            Radius = entry.CollisionRadius;
            LinearDumping = entry.LinearDumping;
            Weapon = Factories.WeaponFactory.Instance.GetEntry(entry.Weapon);

            Stats.SetValue(proto_game.Stats.SkillCooldown, 0.1f);
        }

        public override void InitPhysics(bool dynamicBody = true, bool isSensor = false)
        {
            base.InitPhysics(dynamicBody, isSensor);
            //Body.SetLinearDamping(LinearDumping);
            AddToCollisionMask((ushort)PhysicsDefs.Category.PICKUPS);
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
            appearData.skill = Skill.Entry.Type;

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
            AfterInput();
            statusManager_.Update(dt);
            HP = System.Math.Min(HP + Stats.GetFinValue(proto_game.Stats.HealthRegen) * dt * 0.2f, Stats.GetFinValue(proto_game.Stats.MaxHealth));
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

        public void AddStatus(proto_game.PowerUpType powerUp, float lifetime)
        {
            var status = new Status.Status(powerUp, lifetime);
            statusManager_.Add(status);
        }

        private void AfterInput()
        {
            Body.SetLinearVelocity(Vec2.Zero);
        }

        public bool HasAnyInput()
        {
            return inputs_.Count > 0;
        }

        public void ProcessInput(float dt)
        { 
            var input = Input;
            if (input != null)
            {
                var force = new helpers.Vector2(input.force_x, input.force_y);
                if (!helpers.MathHelper.Approx(force, helpers.Vector2.zero))
                {
                    MoveInDirection(force);
                }
                
                //convert angle to radians
                Rotation = input.rotation * helpers.MathHelper.Deg2Rad;

                if (input.shoot) 
                {
                    PerformAttackAtDirection(Rotation);
                }

                if (input.skill)
                {
                    CastSkill();
                }

                ApplyDumping();
            }

            //process only one tick at time
            input = null;
            if (inputs_.Count > 0)
            {
                input = inputs_[0];
                inputs_.RemoveFromFront();
            }

            Input = input;
        }

        private void ApplyDumping()
        {
            var dumpingCoefficent = helpers.MathHelper.Clamp01(1.0f - GlobalDefs.GetUpdateInterval() * LinearDumping);
            Velocity *= dumpingCoefficent;
            RecoilVelocity *= dumpingCoefficent;
            if (helpers.MathHelper.Approx(Velocity.Length(), 0.0f))
            {
                Velocity = helpers.Vector2.zero;
            }
            if (helpers.MathHelper.Approx(RecoilVelocity.Length(), 0.0f))
            {
                RecoilVelocity = helpers.Vector2.zero;
            }
            Body.SetLinearVelocity(Velocity + RecoilVelocity);
        }

        public void AddInput(proto_game.PlayerInput.Request input)
        {
            if (inputs_.Count == MaxStoredInputs) 
                inputs_.RemoveFromFront();

            if (IsInputValid(input))
            {
                inputs_.AddToBack(input);
            }
        }

        public bool IsInputValid(proto_game.PlayerInput.Request input)
        {
            var maxTickDelta = Game.MaxTickDelta;
            int nMinDelta = System.Math.Max(0, Game.Tick - maxTickDelta);
            int nMaxDelta = Game.Tick + maxTickDelta;

            bool valid = (input.tick >= nMinDelta && input.tick < nMaxDelta);

            return valid || true;
        }
    }
}
