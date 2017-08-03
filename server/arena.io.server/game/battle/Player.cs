using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using Nito;

using shared.factories;
using shared.account;
using shared.helpers;
using arena.common.battle;
using arena.matchmaking;

namespace arena.battle
{
    class Player : Unit, PlayerExperience.IExpProvider
    {
        private static readonly int MaxStoredInputs = 100;
        private static ILogger log = LogManager.GetCurrentClassLogger();

        public class RemoteInputData
        {
            public proto_game.PlayerInput.Request data = null;
            public int attackSyncID = -1;
        }

        //private HashSet<Entity> visibleEntities_ = new HashSet<Entity>();
        private PlayerExperience exp_;
        private Deque<RemoteInputData> inputs_ = new Deque<RemoteInputData>();
        private bool castSkill_ = false;
        private bool shoot_ = false;

        public Player(net.PlayerController controller, Profile profile)
        {
            Controller = controller;
            Category = PhysicsDefs.Category.PLAYER; 
            Level = 1;
            Profile = profile;
            exp_ = new PlayerExperience(this);
            BattleStats = new PlayerBattleStats();
            Input = new RemoteInputData();
            UpgradePointsLeft = 0;
            exp_.OnLevelUp = (int lvl) => UpgradePointsLeft++;
            additionalCollisionMask_ = (ushort)PhysicsDefs.Category.PICKUPS;
        }

        public RemoteInputData Input
        { get; set; }

        public Profile Profile
        { get; private set; }

        public proto_profile.PlayerClasses SelectedClass
        { get; set; }

        public int UpgradePointsLeft
        { get; set; }

        public int Level
        { get; set; }

        public PlayerBattleStats BattleStats
        { get; set; }

        public Room Room
        { get; set; }

        public int Ping
        { get; set; }

        public net.PlayerController Controller
        { get; private set; }

        public void AssignStats() 
        {
            var entry = PlayerClassFactory.Instance.GetEntry(SelectedClass);

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
            AddSkill(entry.Skill);
            Radius = entry.CollisionRadius;
            LinearDumping = entry.LinearDumping;
            Weapon = WeaponFactory.Instance.GetEntry(entry.Weapon);

            Stats.SetValue(proto_game.Stats.SkillCooldown, 0.1f);
            UpgradePointsLeft = Level - 1;
        }

        public void ResetLevel()
        {
            Level = 1;
        }

        public override void InitPhysics()
        {
            base.InitPhysics();
        }

        public override Net.EventPacket GetAppearedPacket()
        {
            var appearData = new proto_game.PlayerAppeared(); 

            appearData.name = Profile.Name;
            appearData.guid = ID;
            appearData.hp = HP;
            appearData.level = Level;
            FillStatsPacket(appearData.stats);
            appearData.position = new proto_game.Vector();

            var pos = Position;
            appearData.position.x = pos.x;
            appearData.position.y = pos.y;
            appearData.@class = SelectedClass;
            appearData.skill = Skill.Entry.Type;

            return ConstructPacket(proto_common.Events.PLAYER_APPEARED, appearData);
        }

        public override void Update(float dt)
        {
            base.Update(dt);

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

        protected override void OnWeaponAttack(AttackData attData)
        {
            base.OnWeaponAttack(attData);     

            Game.SyncAttackWithRemotePlayer(this, attData.FirstBulletID, Input.attackSyncID);
        }

        public void PostUpdate()
        {
            Body.LinearVelocity = Vector2.zero; 

            RemoteInputData input = null;
            //process only one input input at time
            if (inputs_.Count > 0)
            {
                input = inputs_[0];
                inputs_.RemoveFromFront();
            }
            Input = input;
        }

        public bool HasInput()
        {
            return inputs_.Count > 0;
        }

        public void ProcessInput(float dt)
        {
            if (Input == null) 
                return;
            var input = Input.data;
            if (input != null)
            {
                var force = new Vector2(input.force_x, input.force_y);
                MoveInDirection(force);
                //convert angle to radians
                Rotation = input.rotation * MathHelper.Deg2Rad;

                shoot_ = input.shoot;
                castSkill_ = input.skill;

                if (shoot_)
                {
                    PerformAttackAtDirection(Rotation);
                }

                if (castSkill_)
                {
                    CastSkill();
                }

                ApplyDumping();

                Body.LinearVelocity = Velocity + RecoilVelocity;
            }
        }

        private void ApplyDumping()
        {
            var dumpingCoefficent = MathHelper.Clamp01(1.0f - GlobalDefs.GetUpdateInterval() * LinearDumping);
            //Velocity *= dumpingCoefficent;
            RecoilVelocity *= dumpingCoefficent;
            if (MathHelper.Approx(Velocity.Length(), 0.0f))
            {
                Velocity = Vector2.zero;
            }
            if (MathHelper.Approx(RecoilVelocity.Length(), 0.0f))
            {
                RecoilVelocity = Vector2.zero;
            } 
        }

        public void AddInput(proto_game.PlayerInput.Request input, int attSyncID)
        {
            if (inputs_.Count == MaxStoredInputs) 
                inputs_.RemoveFromFront();

            if (IsInputValid(input))
            {
                inputs_.AddToBack(new RemoteInputData() { data = input, attackSyncID = attSyncID });
            }
        }

        public bool IsInputValid(proto_game.PlayerInput.Request input)
        {
            var maxTickDelta = Game.MaxTickDelta;
            int nMinDelta = System.Math.Max(0, Game.Tick - maxTickDelta);
            int nMaxDelta = Game.Tick + maxTickDelta;
            bool valid = (input.tick >= nMinDelta && input.tick < nMaxDelta);
            return valid;
        }
    }
}
