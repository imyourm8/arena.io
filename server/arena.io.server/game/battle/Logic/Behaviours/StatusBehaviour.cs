using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.behaviours
{
    class StatusBehaviour : Behaviour, IStatusBehaviour
    {
        public StatusBehaviour()
        {
            Status = BehaviourStatus.NotStarted;
        }

        BehaviourStatus IStatusBehaviour.Status
        {
            get;
            set;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            Status = BehaviourStatus.NotStarted;
            base.OnEnter(holder, stateHolder, ref behaviorState);
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            
        }
    }
}
