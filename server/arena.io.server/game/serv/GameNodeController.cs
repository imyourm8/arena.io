using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;
using ExitGames.Logging;
using ExitGames.Concurrency.Fibers;

using shared.net;

using Commands = proto_server.Commands;
using Events = proto_server.Events;
using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using OperationHandler = shared.net.OperationHandler<proto_server.Request>;

namespace arena.serv
{
    using matchmaking;
    using battle;
    using serv.perfomance;
    using proto_server;

    class GameNodeController : ServerController
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();
        private GameApplication app_;

        public GameNodeController(GameApplication app)
            :base(app)
        {
            app_ = app;
            app_.ScheduleOnInterval(SendNodeStatus, 1000, 1000);

            AddOperationHandler(Commands.CREATE_REMOTE_GAME, new OperationHandler(HandleCreateNewGame));
        }

        #region Public Methods

        public void Initialize()
        {
            Connection.OnConnectionEstablishedEvent += SendNodeInfo;
        }

        public void OnGameFinished(Game game)
        {
            var evt = new GameFinished();
            evt.id = game.ID.ToString();
            SendEvent(Events.GAME_FINISHED, evt);
        }

        #endregion

        #region Private Methods

        private void SendNodeStatus()
        {
            var nodeStatus = new proto_server.GameNodeStatus();
            nodeStatus.workload_level = app_.WorkloadController.FeedbackLevel;
            nodeStatus.players_connected = (int)Counter.PeerCount.RawValue;

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

        private void HandleCreateNewGame(Request request)
        { 
            var createGameReq = request.Extract<CreateRemoteGame.Request>(Commands.CREATE_REMOTE_GAME);
            app_.GameManager.CreateGame(createGameReq.mode);
        }

        #endregion
    }
}
