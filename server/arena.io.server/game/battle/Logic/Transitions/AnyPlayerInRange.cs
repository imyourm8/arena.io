using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.transitions
{
    class AnyPlayerInRange : Transition
    {
        private readonly float range_;

        public AnyPlayerInRange(string targetState, float range)
            : base(targetState)
        {
            range_ = range;
        }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            var hitTest = target.Game.Map.GetNearestEntities(target.Position, range_, PhysicsDefs.Category.PLAYER);
            return hitTest != null && hitTest.Any();
        }
    }
}
