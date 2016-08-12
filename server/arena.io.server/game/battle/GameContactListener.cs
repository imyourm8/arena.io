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
    class GameContactListener : ContactFilter, ContactListener
    {
        void ContactListener.BeginContact(Contact contact) 
        {
            var body1 = contact.FixtureA.Body;
            var body2 = contact.FixtureB.Body;

            if (HandleBulletContact(body1, body2))
            { }
            else if (HandlePowerUpContact(body1, body2))
            { }
            else if (!DEBUG(body1, body2))
            { }
            else
            {
                int g = 0;
            }
        }

        private bool HandleBulletContact(Body body1, Body body2)
        {
            var bullet1 = body1.GetUserData() as Bullet;
            var bullet2 = body2.GetUserData() as Bullet;

            if (bullet1 != null || bullet2 != null)
            {
                var target = body2.GetUserData() as Entity;
                if (bullet1 == null)
                {
                    bullet1 = bullet2;
                    target = body1.GetUserData() as Entity;
                }

                if (bullet1 != null && target != null)
                {
                    bullet1.OnCollision(target);
                    return true;
                }
            }

            return false;
        }

        private bool HandlePowerUpContact(Body body1, Body body2)
        {
            var power1 = body1.GetUserData() as PowerUp;
            var power2 = body2.GetUserData() as PowerUp;

            if (power1 != null || power2 != null)
            {
                var target = body2.GetUserData() as Player;
                if (power1 == null)
                {
                    power1 = power2;
                    target = body1.GetUserData() as Player;
                }

                if (power1 != null && target != null)
                {
                    power1.OnPickUpBy(target);
                    return true;
                }
            }

            return false;
        }

        private bool DEBUG(Body body1, Body body2)
        {
            var power1 = body1.GetUserData() as Player;
            var power2 = body2.GetUserData() as Player;

            if (power1 != null || power2 != null)
            {
                var isWall = body2.GetFixtureList().Filter.CategoryBits;
                if (power1 == null)
                {
                    power1 = power2;
                    isWall = body1.GetFixtureList().Filter.CategoryBits;
                }

                if (power1 != null && isWall ==(ushort) PhysicsDefs.Category.WALLS)
                {
                    return false;
                }
            }

            return true;
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

            return result && base.ShouldCollide(fixtureA, fixtureB);
        }
    }
}
