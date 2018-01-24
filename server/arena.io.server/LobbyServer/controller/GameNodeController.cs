using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;

using shared.net;
using shared;

using proto_server;
using proto_game;

using Commands = proto_server.Commands;
using Events = proto_server.Events;
using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using RequestHandler = shared.net.OperationHandler<proto_server.Request>;
using ResponseHandler = shared.net.OperationHandler<proto_server.Response>;
using EventHandler = shared.net.OperationHandler<proto_server.Event>;

namespace LobbyServer.controller
{
    using load_balancing;
    using shared.net.interfaces;
    using proto_server;
    using matchmaking;
    using matchmaking.interfaces;

    public class GameNodeController : ServerController
    {
        #region Fields

        private Application application_;
        private GameNode node_;

        #endregion

        public GameNodeController(Application app)
            : base(app)
        {
            application_ = app;

            AddOperationHandler(Commands.REGISTER_GAME_NODE, new RequestHandler(HandleRegisterGameNode));

            AddEventHandler(Events.NODE_STATUS, new EventHandler(HandleNodeStatus));
            AddEventHandler(Events.GAME_FINISHED, new EventHandler(HandleGameFinished));
        }

        public override void HandleDisconnect()
        {
            base.HandleDisconnect();

            if (node_ != null)
            {
                application_.Loadbalancer.RemoveGameNode(node_);
            }
        }

        #region Public Methods

        public void Disconnect()
        {
            Connection.Disconnect();
        }

        public void CreateGame(GameMode mode)
        {
            var request = new CreateRemoteGame.Request();
            request.mode = mode;

            var responseHandler = new ResponseHandler((Response response) => 
            { 
                if (response.error != 0)
                {
                    return;
                }
                var gameResponse = response.Extract<CreateRemoteGame.Response>(Commands.CREATE_REMOTE_GAME);
                var session = new GameSession(mode, gameResponse.id, gameResponse.max_players_allowed, gameResponse.min_players_to_start, node_);
                application_.GameManager.Add(session);
            });

            SendRequest(Commands.CREATE_REMOTE_GAME, request, responseHandler);
        }

        public void SendPlayersJoin(List<string> playersId, string gameId, Action<bool> callback)
        {
            var req = new PlayersJoin.Request();
            req.players.AddRange(playersId);
            req.game_id = gameId;
            SendRequest(Commands.PLAYERS_JOIN, req, new ResponseHandler((Response response) =>
                {
                    callback(response.error == 0);
                }));
        }

        #endregion

        #region Request handlers

        private void HandleRegisterGameNode(Request request)
        {
            RegisterGameNode.Request req = request.Extract<RegisterGameNode.Request>(Commands.REGISTER_GAME_NODE);
            GameNode node = new GameNode(req.id, req.ip, new SimpleGameList());
            application_.Loadbalancer.AddGameNode(node);
            node_ = node;
        }

        #endregion

        #region Event Handlers

        private void HandleNodeStatus(Event evt)
        {
            GameNodeStatus status = evt.Extract<GameNodeStatus>(Events.NODE_STATUS);
            application_.Loadbalancer.UpdateStatus(node_.Id, status);
        }

        private void HandleGameFinished(Event evt)
        {
            GameFinished gameEvt = evt.Extract<GameFinished>(Events.GAME_FINISHED);
            application_.GameManager.Remove(node_, gameEvt.id);
        }

        #endregion
    }
}
