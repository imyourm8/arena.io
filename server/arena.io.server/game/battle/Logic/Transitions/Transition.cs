using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle.Logic.States;

namespace arena.battle.Logic.Transitions
{
    abstract class Transition : ITransition
    {
        private string targetStateName_;
        private State TargetState
        { get; set; }

        public Transition(string targetStateName)
        {
            targetStateName_ = targetStateName; 
        }

        public void Update(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            var state = stateHolder.GetState(this); 

            if (TryToPerformTransition(manager.Host, dt, ref state))
            {
                manager.SwitchTo(TargetState);
            }

            if (state == null)
            {
                stateHolder.RemoveState(this);
            }
            else
            {
                stateHolder.SaveState(this, state);
            }
        }

        protected abstract bool TryToPerformTransition(Entity target, float dt, ref object state);

        protected virtual void HandleEnter(Entity target, ref object state)
        { }

        protected virtual void HandleExit(Entity target, ref object state)
        { }

        public void Resolve(Dictionary<string, State> states)
        {
            TargetState = states[targetStateName_];
        }

        void ILogicElement.OnEnter(Entity target, IStateStorage stateHolder)
        {
            object state = null;
            HandleEnter(target, ref state);

            if (state != null)
            {
                stateHolder.SaveState(this, state);
            }
            else
            {
                stateHolder.RemoveState(this);
            }
        }

        void ILogicElement.OnExit(Entity target, IStateStorage stateHolder)
        {
            object state = stateHolder.GetState(this);
            HandleExit(target, ref state);

            if (state != null)
            {
                stateHolder.SaveState(this, state);
            }
            else
            {
                stateHolder.RemoveState(this);
            }
        }
    }
}
