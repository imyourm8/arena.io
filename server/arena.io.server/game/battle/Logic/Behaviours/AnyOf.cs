using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.behaviours
{
    class AnyOf : Behaviour
    {
        class AnyOfState
        {
            public int currentBehaviourIndex;
        }

        private IStatusBehaviour[] behaviours_ = null;

        public AnyOf(params IStatusBehaviour[] behaviours)
        {
            behaviours_ = behaviours;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            base.OnEnter(holder, stateHolder, ref behaviorState);
            AnyOfState state = new AnyOfState();
            state.currentBehaviourIndex = -1;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            AnyOfState anyOfState = state as AnyOfState;
            if (anyOfState.currentBehaviourIndex < 0)
            {
                //select state to update
                int i = 0;
                while (i < behaviours_.Length)
                {
                    IStatusBehaviour behaviour = behaviours_[anyOfState.currentBehaviourIndex];
                    behaviour.Update(StateManager, StateHolder, dt);

                    if (behaviour.Status == BehaviourStatus.Started)
                    {
                        anyOfState.currentBehaviourIndex = i;
                        break;
                    }
                }
            }
            else
            {
                IStatusBehaviour behaviour = behaviours_[anyOfState.currentBehaviourIndex];
                behaviour.Update(StateManager, StateHolder, dt);
                if (behaviour.Status == BehaviourStatus.NotStarted || behaviour.Status == BehaviourStatus.Finished)
                {
                    anyOfState.currentBehaviourIndex = -1;
                    //next cycle find new behaviour
                }
            }
        }
    }
}
