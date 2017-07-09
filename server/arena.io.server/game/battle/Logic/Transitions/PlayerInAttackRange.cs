using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Transitions
{
    class PlayerInAttackRange : Transition
    {
        public PlayerInAttackRange(string targetState):base(targetState)
        { }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            var unit = target as Unit;
            if (unit == null) return false;

            return unit.Game.Map.GetNearestEntities(unit.Position, 1.0f).FirstOrDefault() != null;
        }
    }
}
