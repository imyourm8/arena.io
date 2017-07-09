using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic
{
    enum BehaviourStatus
    { 
        NotStarted,
        Started,
        Finished
    }

    interface IBehaviour : ILogicElement
    {
        
    }

    interface IStatusBehaviour : IBehaviour
    {
        BehaviourStatus Status { get; set; }
    }
}
