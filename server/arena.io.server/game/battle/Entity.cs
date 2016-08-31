using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using arena.helpers;
using Box2DX.Dynamics;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

namespace arena.battle
{
    class Entity : SpatialHash.IEntity
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        private Attributes.UnitAttributes stats_ = new Attributes.UnitAttributes();
        public Attributes.UnitAttributes Stats
        { get { return stats_; } }

        public Entity()
        {
            TrackSpatially = true;
        }

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
                Body.SetPosition(position_);
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

        public Vector2 Position
        {
            get 
            {
                if (Body != null)
                {
                    position_ = Body.GetPosition();
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

        public proto_game.UnitDie GetDiePacket()
        {
            proto_game.UnitDie diePck = new proto_game.UnitDie();
            diePck.guid = ID;
            return diePck;
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
            Body.SetLinearVelocity(Velocity);
        }

        public virtual void InitPhysics(bool dynamicBody = true, bool isSensor = false)
        {
            if (Body != null)
                return;

            BodyDef def = new BodyDef();
            def.FixedRotation = true;
            
            Body body = Game.CreateBody(def); 

            CircleDef shape = new CircleDef();
            shape.Radius = Radius;
            shape.Density = dynamicBody?1.0f:0.0f;//dynamic body
            shape.Filter.CategoryBits = (ushort)Category;
            shape.IsSensor = isSensor;

            ushort mask = (ushort)PhysicsDefs.Category.WALLS;
            shape.Filter.MaskBits = mask;
            body.CreateFixture(shape); 
            body.SetMassFromShapes();
            body.SetUserData(this);
            //refresh body's position just in case
            var pos = Position;
            Body = body;
            Position = pos;
            PrevPosition = pos;   
        }

        public virtual void PostUpdate()
        { }

        public void AddToCollisionMask(ushort mask)
        {
            var filter = Body.GetFixtureList().Filter;
            filter.MaskBits |= mask;
            Body.GetFixtureList().Filter = filter;        
        }
    }
}
