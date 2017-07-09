using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Transitions
{
    class NoPlayerInRange : Transition
    {
        private float radius_ = 0.0f;
        public NoPlayerInRange(string stateName, float radius):base(stateName)
        {
            radius_ = radius;
        }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            return !target.Game.Map.GetNearestEntities(target.Position, radius_, PhysicsDefs.Category.PLAYER).Any();
        }
    }
}
