using System;
using System.Collections.Generic;
using arena.battle.logic.states;

namespace arena.battle.logic
{
    class StateManager : IStateStorage, IStateManager, IDisposable
    {
        private State currenState_ = null;
        private State nextState_ = null;
        private Entity holder_ = null;
        private State currentStateCommonRoot_ = null;
        private IDictionary<logic.ILogicElement, object> StateStorage { get; set; }

        public StateManager(Entity holder, State initialState = null)
        {
            StateStorage = new Dictionary<ILogicElement, object>();
            holder_ = holder;
            nextState_ = initialState;
        }

        Entity IStateManager.Host
        {
            get { return holder_; }
        }

        public IState CurrentState 
        {
            get { return currenState_; }
        }

        public void SwitchTo(State state)
        {
            if (currenState_ != state)
            {
                nextState_ = state;
            }
        }

        public void Update(float dt)
        {
            if (nextState_ != null)
            {
                //while (nextState_.States.Count > 0)
                //    nextState_ = nextState_.States[0];

                //perform state switch
                if (currenState_ != null)
                {
                    currenState_.OnExit(holder_, this, currentStateCommonRoot_);
                }

                currentStateCommonRoot_ = State.CommonParent(nextState_, currenState_);
                nextState_.OnEnter(holder_, this, currentStateCommonRoot_);
                currenState_ = nextState_;
                nextState_ = null;
            }

            if (currenState_ != null)
            {
                currenState_.Update(this, this, dt);
            }
        }

        #region IStateHolder implementation
        object IStateStorage.GetState(ILogicElement key)
        {
            object data = null;
            StateStorage.TryGetValue(key, out data);
            return data;
        }

        void IStateStorage.SaveState(ILogicElement key, object data)
        {
            StateStorage[key] = data;
        }

        void IStateStorage.RemoveState(ILogicElement key)
        {
            StateStorage.Remove(key);
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            StateStorage.Clear();
        }
        #endregion
    }
}
