using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.transitions
{
    class MoveToState : Transition
    {
        public MoveToState(string stateName)
            : base(stateName)
        { }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            return true;
        }
    }
}
