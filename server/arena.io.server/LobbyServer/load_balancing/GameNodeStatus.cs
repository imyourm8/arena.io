using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.load_balancing
{
    class GameNodeStatus
    {
        public int GameSessions { get; set; }
        public int PlayerConnected { get; set; }
        public int CpuLoad { get; set; }
    }
}
