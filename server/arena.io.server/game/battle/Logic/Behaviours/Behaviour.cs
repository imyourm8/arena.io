using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Behaviours
{
    abstract class Behaviour : IBehaviour
    {
        void ILogicElement.OnEnter(Entity target, IStateStorage stateHolder)
        {
            Status = BehaviourStatus.NotStarted;
            object stateSave = null;
            OnEnter(target, stateHolder, ref stateSave);
            HandleStateChange(stateHolder, stateSave);
        }

        void ILogicElement.OnExit(Entity target, IStateStorage stateHolder)
        {
            var stateSave = stateHolder.GetState(this); 
            OnExit(target, stateHolder, ref stateSave);
            HandleStateChange(stateHolder, stateSave);
        }

        private void HandleStateChange(IStateStorage stateHolder, object stateSave)
        {
            if (stateSave == null)
            {
                stateHolder.RemoveState(this);
            }
            else
            {
                stateHolder.SaveState(this, stateSave);
            }
        }

        protected virtual void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        { }

        protected virtual void OnExit(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        { }

        public void Update(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            StateHolder = stateHolder;
            StateManager = manager;
            var state = stateHolder.GetState(this);
            HandleUpdate(manager.Host, dt, ref state);
            HandleStateChange(stateHolder, state);
        }

        protected IStateStorage StateHolder
        {
            get;
            set;
        }

        protected IStateManager StateManager
        {
            get;
            set;
        }

        abstract protected void HandleUpdate(Entity target, float dt, ref object state);

        public BehaviourStatus Status
        {
            get;
            set;
        }
    }
}
