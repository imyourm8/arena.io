using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.serv.load_balancing
{
    public enum ServerState
    {
        Normal = 0,

        OutOfRotation = 1,

        Offline = 2
    }
}
