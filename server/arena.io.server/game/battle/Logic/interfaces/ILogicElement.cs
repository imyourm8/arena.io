using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic
{
    interface ILogicElement
    {
        void Update(IStateManager manager, IStateStorage stateHolder, float dt);
        void OnEnter(Entity target, IStateStorage stateHolder);
        void OnExit(Entity target, IStateStorage stateHolder);
    }
}
