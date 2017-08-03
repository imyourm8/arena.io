using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;

using System.Collections.Generic;
using shared.net;

namespace LobbyServer.controller
{
    public class ServerController : ServerRequestHandler
    {
        public ServerController(ServerConnection conn)
            : base(conn)
        {
        }
    }
}
