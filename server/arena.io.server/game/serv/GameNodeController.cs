using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;
using ExitGames.Logging;
using ExitGames.Concurrency.Fibers;

using shared.net;
using arena.serv.load_balancing;
using arena.serv.perfomance;

using Commands = proto_server.Commands;
using Events = proto_server.Events;
using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using OperationHandler = shared.net.OperationHandler<proto_server.Request>;

namespace arena.serv
{
    class GameNodeController : ServerController
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        private IFiber fiber_;
        private Application app_;

        public GameNodeController(Application app):
            base()
        {
            app_ = app;
            fiber_ = new PoolFiber();
            fiber_.Start();

            fiber_.ScheduleOnInterval(SendNodeStatus, 1000, 1000);
        }

        #region Private Methods
        void SendNodeStatus()
        {
            var nodeStatus = new proto_server.GameNodeStatus();
            nodeStatus.workload_level = app_.WorkloadController.FeedbackLevel;
            nodeStatus.players_connected = (int)Counter.PeerCount.RawValue;
            //nodeStatus.active_rooms
            SendEvent(Events.NODE_STATUS, nodeStatus);
        }
        #endregion

        #region Handlers
        #endregion
    }
}
