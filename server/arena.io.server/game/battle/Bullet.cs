using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2DX.Dynamics;
using Box2DX.Common;

namespace arena.battle
{
    class Bullet : Entity
    {
        public Bullet()
        {
            Category = PhysicsDefs.Category.BULLET;
        }

        public Unit Owner
        { get; set; }

        public BulletEntry Entry
        { get; set; }

        public override void InitPhysics(bool dynamicBody = true)
        {
            BodyDef def = new BodyDef();
            Body body = Game.CreateBody(def);

            CircleDef shape = new CircleDef();
            shape.Radius = Entry.Radius;
            shape.Density = 1.0f;
            shape.Filter.CategoryBits = (ushort)Category;
            shape.Filter.GroupIndex = -1;//disable collision between bullets
            shape.IsSensor = true;

            ushort mask = (ushort)PhysicsDefs.Category.EXP_BLOCK;
            switch ((ushort)Owner.Category)
            {
                case (ushort)PhysicsDefs.Category.MOB:
                    mask |= (ushort)PhysicsDefs.Category.PLAYER;
                    break;

                case (ushort)PhysicsDefs.Category.PLAYER:
                    mask |= (ushort)PhysicsDefs.Category.MOB | (ushort)PhysicsDefs.Category.PLAYER;
                    break;
            }
            shape.Filter.MaskBits = mask;
            body.CreateFixture(shape);
            body.SetMassFromShapes();
            body.SetUserData(this);
            Body = body;
        }

        public void OnCollision(Entity target)
        {
            if (target != Owner)
            {
                target.ApplyDamage(Owner, Stats.GetFinValue(proto_game.Stats.BulletDamage));
            }
        }
    }
}
