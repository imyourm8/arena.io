using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle.Logic.Transitions;
using arena.battle.Logic.Behaviours;

namespace arena.battle.Logic.States
{
    /*
        State can hold other states
        State consists of Behaviours and Transitions which are evaluated inside Update 
     */
    class State : IState
    {
        public static readonly State NullState = new State();

        public string Name
        { get; private set; }

        public State Parent
        { get; private set; }

        private List<State> ChildrenStates
        { get; set; }

        private IList<Transition> Transitions
        { get; set; }

        private IList<Behaviour> Behaviours
        { get; set; }

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

            foreach (var child in children)
            {
                if (child is State)
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

        public void Init()
        {
            Dictionary<string, State> states = new Dictionary<string, State>();
            //recursively get all states as Name => State pairs
            FillStates(states);
            //now give this info to behaviours and transitions recursively too
            ResolveTransitions(states);
        }

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

#region ILogicElement Implementation
        public void Update(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            var s = this;
            while (s != null)
            {
                foreach (var b in s.Behaviours)
                {
                    b.Update(manager, stateHolder, dt);
                }

                s = s.Parent;
            }

            foreach (var t in Transitions)
            {
                t.Update(manager, stateHolder, dt);
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
