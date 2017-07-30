using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.transitions
{
    class TimedTransition : Transition
    {
        class TimedtransitionState
        {
            public float delay;
        }

        private readonly float delay_;

        public TimedTransition(string targetState, float delay)
            : base(targetState)
        {
            delay_ = delay;
        }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            var timedState = state as TimedtransitionState;
            timedState.delay -= dt;
            return timedState.delay <= 0.0f;
        }

        protected override void HandleEnter(Entity target, ref object state)
        {
            var timedState = new TimedtransitionState();
            timedState.delay = delay_;
            state = timedState;
        }
    }
}
