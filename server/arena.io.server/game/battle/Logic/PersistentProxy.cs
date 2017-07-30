using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic
{
    class PersistentProxy : IPersistent
    {
        private ILogicElement element_;

        public PersistentProxy(ILogicElement element)
        {
            element_ = element;
        }

        void ILogicElement.Update(IStateManager manager, IStateStorage stateHolder, float dt)
        {
            element_.Update(manager, stateHolder, dt);
        }

        void ILogicElement.OnEnter(Entity target, IStateStorage stateHolder)
        {
            element_.OnEnter(target, stateHolder);
        }

        void ILogicElement.OnExit(Entity target, IStateStorage stateHolder)
        {
            element_.OnEnter(target, stateHolder);
        }
    }
}
