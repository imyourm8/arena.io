using arena.battle.logic.states;

namespace arena.battle.logic
{
    interface IStateManager
    {
        void SwitchTo(State state);
        IState CurrentState { get; }
        Entity Host { get; }
    }
}
