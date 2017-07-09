using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2CS;

namespace arena.battle
{
    class GameContactListener : ContactListener
    {
        public override void BeginContact(Contact contact) 
        {
            var body1 = contact.FixtureA.Body;
            var body2 = contact.FixtureB.Body;

            if (HandleBulletContact(body1, body2))
            { }
            //else if (HandlePowerUpContact(body1, body2))
            //{ }
            else if (HandlePickUpContact(body1, body2))
            { }
        }

        private bool HandleBulletContact(Body body1, Body body2)
        {
            var bullet1 = body1.UserData as Bullet;
            var bullet2 = body2.UserData as Bullet;

            if (bullet1 != null || bullet2 != null)
            {
                var target = body2.UserData as Entity;
                if (bullet1 == null)
                {
                    bullet1 = bullet2;
                    target = body1.UserData as Entity;
                }

                if (bullet1 != null && target != null)
                {
                    bullet1.OnCollision(target);
                    return true;
                }
            }

            return false;
        }

        private bool HandlePickUpContact(Body body1, Body body2)
        {
            var pickup1 = body1.UserData as PickUp;
            var pickup2 = body2.UserData as PickUp;

            if (pickup1 != null || pickup2 != null)
            {
                var target = body2.UserData as Player;
                if (pickup1 == null)
                {
                    pickup1 = pickup2;
                    target = body1.UserData as Player;
                }

                if (pickup1 != null && target != null)
                {
                    pickup1.OnPickUpBy(target);
                    return true;
                }
            }

            return false;
        }

        private bool HandlePowerUpContact(Body body1, Body body2)
        {
            var power1 = body1.UserData as PowerUp;
            var power2 = body2.UserData as PowerUp;

            if (power1 != null || power2 != null)
            {
                var target = body2.UserData as Player;
                if (power1 == null)
                {
                    power1 = power2;
                    target = body1.UserData as Player;
                }

                if (power1 != null && target != null)
                {
                    power1.OnPickUpBy(target);
                    return true;
                }
            }

            return false;
        }

        public override void EndContact(Contact contact)
        {
            
        }

        public override void PostSolve(Contact contact, ContactImpulse impulse) 
        {
            
        }

        public override void PreSolve(Contact contact, Manifold oldManifold)
        {
        }
    }
}
