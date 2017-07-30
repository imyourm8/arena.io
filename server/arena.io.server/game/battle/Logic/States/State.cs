using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle.logic.transitions;
using arena.battle.logic.behaviours;

namespace arena.battle.logic.states
{
    /*
        State can hold other states
        State consists of Behaviours and Transitions which are evaluated inside Update 
     */
    class State : IState
    {
        public static readonly State NullState = new State();

        private List<State> ChildrenStates
        { get; set; }

        private IList<Transition> Transitions
        { get; set; }

        private IList<Behaviour> Behaviours
        { get; set; }

        private IList<IPersistent> PersistentLogic
        { get; set; }

        public string Name
        { get; private set; }

        public State Parent
        { get; private set; }

        public IReadOnlyList<State> States
        { get { return ChildrenStates; } }

        public static State CommonParent(State a, State b)
        {
            if (a == null || b == null) return null;
            return _CommonParent(a, a, b);
        }

        private static State _CommonParent(State current, State a, State b)
        {
            if (b.Is(current)) return current;
            if (a.Parent == null) return null;
            return _CommonParent(current.Parent, a, b);
        }

        //child is parent
        //parent is not child
        public bool Is(State state)
        {
            if (this == state) return true;
            if (Parent != null) return Parent.Is(state);
            return false;
        }

        public State(params ILogicElement[] children)
            : this("", children)
        { }

        public State(string name, params ILogicElement[] children)
        {
            Name = name;
            ChildrenStates = new List<State>();
            Transitions = new List<Transition>();
            Behaviours = new List<Behaviour>();
            PersistentLogic = new List<IPersistent>();

            foreach (var child in children)
            {
                if (child is IPersistent)
                {
                    PersistentLogic.Add(child as IPersistent);
                }
                else if (child is State)
                {
                    var state = child as State;
                    state.Parent = this;
                    ChildrenStates.Add(state);
                }
                else if (child is Transition)
                {
                    Transitions.Add(child as Transition);
                }
                else if (child is Behaviour)
                {
                    Behaviours.Add(child as Behaviour);
                }
            }
        }

#region Public methods
        public void Init()
        {
            Dictionary<string, State> states = new Dictionary<string, State>();
            //recursively get all states as Name => State pairs
            FillStates(states);
            //now give this info to behaviours and transitions recursively too
            ResolveTransitions(states);
        }

        public void FullUpdate(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            foreach (var b in Behaviours)
            {
                b.Update(manager, stateHolder, dt);
            }

            foreach (var t in Transitions)
            {
                t.Update(manager, stateHolder, dt);
            }
        }

        public void PersistenOnlyUpdate(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            foreach (var p in PersistentLogic)
            {
                p.Update(manager, stateHolder, dt);
            }
        }
#endregion

#region Private methods
        private void ResolveTransitions(Dictionary<string, State> states)
        {
            foreach (var t in Transitions)
            {
                t.Resolve(states);
            }

            foreach (var s in ChildrenStates)
            {
                s.ResolveTransitions(states);
            }
        }

        private void FillStates(Dictionary<string, State> states)
        {
            states[Name] = this;
            foreach (var s in ChildrenStates)
            {
                s.FillStates(states);
            }
        }
#endregion

#region ILogicElement Implementation
        public void Update(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            if (manager.CurrentState == this)
            {
                FullUpdate(manager, stateHolder, dt);
            }
        }

        public void OnEnter(Entity target, IStateStorage stateHolder)
        {}

        public void OnExit(Entity target, IStateStorage stateHolder)
        {}

        public void OnEnter(Entity target, IStateStorage stateHolder, State commonParent)
        {
            var s = this;
            while (s != null && s != commonParent)
            {
                foreach (ILogicElement b in s.Behaviours)
                {
                    b.OnEnter(target, stateHolder);
                }

                foreach (ILogicElement b in s.Transitions)
                {
                    b.OnEnter(target, stateHolder);
                }
                s = s.Parent;
            }
        }

        public void OnExit(Entity target, IStateStorage stateHolder, State commonParent)
        {
            var s = this;
            while (s != null && s != commonParent)
            {
                foreach (ILogicElement b in s.Behaviours)
                {
                    b.OnExit(target, stateHolder);
                }

                foreach (ILogicElement b in s.Behaviours)
                {
                    b.OnExit(target, stateHolder);
                }
                s = s.Parent;
            }
        }
#endregion
    }
}
