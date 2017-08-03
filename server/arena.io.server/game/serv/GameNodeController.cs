using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;
using ExitGames.Logging;

using shared.net;

using Commands = proto_server.Commands;
using Request = proto_server.Request;
using Response = proto_server.Response;
using OperationHandler = shared.net.OperationHandler<proto_server.Request>;

namespace arena.serv
{
    class GameNodeController : ServerController
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        public GameNodeController(ServerConnection conn):
            base(conn)
        {
            
        }

        #region Handlers

        
        #endregion
    }
}
