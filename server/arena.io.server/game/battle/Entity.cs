using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;
using arena.battle.Logic;
using arena.common.battle;
using arena.battle.Logic.Statuses;
using arena.battle.Net;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using Box2CS;

namespace arena.battle
{
    class Entity : NetworkEntity, SpatialHash.IEntity
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        private StateManager stateManager_;
        private StatusManager statusManager_;
        protected bool dynamicBody_ = true;
        protected bool sensorBody_ = false;
        protected ushort additionalCollisionMask_ = 0;
            
        private Attributes.UnitAttributes stats_ = new Attributes.UnitAttributes();
        public Attributes.UnitAttributes Stats
        { get { return stats_; } }

        public Entity()
        {
            TrackSpatially = true;
            ReplicateOnClients = false;
            stateManager_ = new StateManager(this);
            statusManager_ = new StatusManager(this);
        }

        public bool ReplicateOnClients
        { get; set; }

        public int ID
        { get; set; }

        public bool TrackSpatially
        { get; set; }

        public float Radius
        { get; set; }

        public PhysicsDefs.Category Category
        { get; set; }

        private Vector2 position_ = Vector2.zero;
        public void SetPosition(float x, float y)
        {
            position_.x = x;
            position_.y = y;
            if (Body != null)
            {
                Body.Position = position_;
            }
        }

        private Vector2 prevPosition1_;
        private Vector2 prevPosition2_;
        private Vector2 prevPosition_;

        public Vector2 PrevPosition
        {
            get { return prevPosition_; }
            private set { prevPosition_ = value; }
        }

        public void SetRootState(Logic.States.State state)
        {
            stateManager_.SwitchTo(state);
        }

        public Vector2 Position
        {
            get 
            {
                if (Body != null)
                {
                    position_ = Body.Position;
                }
                return position_;
            }
            set 
            {
                SetPosition(value.x, value.y);
            }
        }

        private Vector2 velocity_ = Vector2.zero;
        public Vector2 Velocity
        {
            get { return velocity_; }
            set
            {
                velocity_ = value;
            }
        }

        private float rotation_;
        public float Rotation
        {
            get { return rotation_; }
            set 
            {
                rotation_ = value;
                RotationVec = new Vector2((float)Math.Cos(rotation_), (float)Math.Sin(rotation_));
            }
        }

        public float LinearDumping
        { get; set; }

        public Vector2 RotationVec
        { get; set; }

        public void SetRotation(Vector2 dir)
        {
            dir.Normilize();
            Rotation = (float)Math.Atan2(dir.y, dir.x);
        }

        public bool Moved
        {
            get
            {
                return !MathHelper.Approx(PrevPosition, Position);
            }
        }

        public float HP
        { get; set; }

        public int Exp
        { get; set; }

        public Game Game
        { get; set; }

        public Body Body
        { get; set; }

        public bool IsAlive
        {
            get { return HP > 0.1f; }
        }

        public void StopMovement()
        {
            Velocity = Vector2.zero;
        }

        public void FillStatsPacket(List<proto_game.StatValue> stats)
        {
            foreach (var stat in Stats)
            {
                var s = new proto_game.StatValue();
                s.step = stat.Step;
                s.stat = stat.ID;
                s.value = stat.FinalValue;
                stats.Add(s);
            }
        }

        public virtual void OnRemove()
        {
        }

        public virtual void Update(float dt)
        {
            Game.Map.RefreshHashPosition(this);
            //hacky way to keep previous position with box2d
            if (Game.Tick % 2 == 0)
            {
                prevPosition2_ = Position;
                prevPosition_ = prevPosition1_;
            }
            else
            {
                prevPosition1_ = Position;
                prevPosition_ = prevPosition2_;
            }

            stateManager_.Update(dt);
            statusManager_.Update(dt);
        }

        public void ApplyDamage(Entity attacker, float damage) 
        {
            if (!IsAlive) 
                return; 

            HP -= Math.Max(damage - Stats.GetFinValue(proto_game.Stats.Armor), 1);

            if (this is Player)
            {
                HP = Math.Max(1.0f, HP);
            }

            var dmgDonePacket = new proto_game.DamageDone();
            dmgDonePacket.hp_left = HP;
            dmgDonePacket.target = ID;

            Game.Broadcast(proto_common.Events.DAMAGE_DONE, dmgDonePacket);

            if (!IsAlive)
            {
                Game.RegisterAsDead(attacker, this);
            }
        }

        public void MoveInDirection(Vector2 dir)
        {
            dir.Normilize();
            dir.Scale(Stats.GetFinValue(proto_game.Stats.MovementSpeed));
            Velocity = dir;

            if (Body != null)
                Body.LinearVelocity = Velocity;
        }

        public virtual void InitPhysics()
        {
            if (Body != null)
                return;

            BodyDef def = new BodyDef();
            def.FixedRotation = true;
            def.BodyType = dynamicBody_ ? BodyType.Dynamic : BodyType.Static;
            
            Body body = Game.Map.CreateBody(def);

            CircleShape shape = new CircleShape();
            shape.Radius = Radius;

            FixtureDef fixture = new FixtureDef(shape);
            fixture.Filter.CategoryBits = (ushort)Category;
            fixture.IsSensor = sensorBody_;

            ushort mask = (ushort)PhysicsDefs.Category.WALLS;
            mask |= additionalCollisionMask_;
            fixture.Filter.MaskBits = mask;
            body.UserData = this;
            body.CreateFixture(fixture);
            //refresh body's position just in case
            var pos = Position;
            Body = body;
            Position = pos;
            PrevPosition = pos;   
        }

        public void AddStatus(GameStatus status)
        {
            statusManager_.Add(status);
        }

        public void RemoveStatus(GameStatus status)
        {
            statusManager_.Remove(status);
        }
    }
}
