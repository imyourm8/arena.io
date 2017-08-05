using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;

using System.Collections.Generic;
using shared.net;

namespace LobbyServer.controller
{
    public class GameNodeController : ServerController
    {
        public GameNodeController(ServerConnection conn)
            : base(conn)
        {
        }
    }
}
