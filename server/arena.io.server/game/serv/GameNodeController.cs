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
    using proto_server;

    class GameNodeController : ServerController
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        private IFiber fiber_;
        private GameApplication app_;

        public GameNodeController(GameApplication app)
            :base()
        {
            app_ = app;
            fiber_ = new PoolFiber();
            fiber_.Start();

            fiber_.ScheduleOnInterval(SendNodeStatus, 1000, 1000);
        }

        #region Public Methods



        #endregion

        #region Private Methods

        private void SendNodeStatus()
        {
            var nodeStatus = new proto_server.GameNodeStatus();
            nodeStatus.workload_level = app_.WorkloadController.FeedbackLevel;
            nodeStatus.players_connected = (int)Counter.PeerCount.RawValue;
            var gameList = GameManager.Instance.GetGameList();
            foreach (var game in gameList)
            {
                var session = new GameSession();
                session.game_id = game.ID.ToString();
                session.closed = game.IsClosed;
                nodeStatus.games.Add(session);
            }

            SendEvent(Events.NODE_STATUS, nodeStatus);
        }

        private void SendNodeInfo()
        {
            var request = new RegisterGameNode.Request();
            request.id = app_.ServerId.ToString();
            request.ip = app_.Ip;

            SendRequest(Commands.REGISTER_GAME_NODE, request);
        }

        #endregion

        #region Handlers

        #endregion

        #region Internal

        internal void Initialize()
        {
            Connection.OnConnectionEstablishedEvent += SendNodeInfo;
        } 

        #endregion
    }
}
