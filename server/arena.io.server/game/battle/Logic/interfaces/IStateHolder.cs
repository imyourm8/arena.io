using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic
{
    interface IStateStorage
    {
        object GetState(ILogicElement key);
        void SaveState(ILogicElement key, object data);
        void RemoveState(ILogicElement key);
    }
}
