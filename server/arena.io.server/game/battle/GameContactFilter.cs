using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2CS;

namespace arena.battle
{
    class GameContactFilter : ContactFilter
    {
        public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            var bulletA = fixtureA.Body.UserData as Bullet;
            var bulletB = fixtureB.Body.UserData as Bullet;

            bool result = false;
            if (bulletA == null || bulletB == null)
                result = true;

            return result && base.ShouldCollide(fixtureA, fixtureB);
        }
    }
}
