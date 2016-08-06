using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Dynamics.Controllers;
using Box2DX.Collision;

namespace arena.battle
{
    class BulletContactListener : ContactFilter, ContactListener
    {
        void ContactListener.BeginContact(Contact contact)
        {
            var bullet = contact.FixtureA.Body.GetUserData() as Bullet;
            Entity target = contact.FixtureB.Body.GetUserData() as Entity;
            
            if (bullet == null)
            {
                bullet = contact.FixtureB.Body.GetUserData() as Bullet;
                target = contact.FixtureA.Body.GetUserData() as Entity;
            }

            if (bullet != null && target != null)
            {
                bullet.OnCollision(target);
            }
        }

        void ContactListener.EndContact(Contact contact)
        {
            
        }

        void ContactListener.PostSolve(Contact contact, ContactImpulse impulse)
        {
            
        }

        void ContactListener.PreSolve(Contact contact, Box2DX.Collision.Manifold oldManifold)
        {
        }

        public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            var bulletA = fixtureA.Body.GetUserData() as Bullet;
            var bulletB = fixtureB.Body.GetUserData() as Bullet;

            bool result = false;
            if (bulletA == null || bulletB == null)
                result = true;

            if ((fixtureB.Body.GetUserData() is ExpBlock)
                || (fixtureA.Body.GetUserData() is ExpBlock))
            {
                int h = 0;
            }

            return result && base.ShouldCollide(fixtureA, fixtureB);
        }
    }
}
