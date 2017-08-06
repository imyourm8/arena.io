using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;

using System.Collections.Generic;
using shared.net;
using shared;

using Commands = proto_server.Commands;
using Events = proto_server.Events;
using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using OperationHandler = shared.net.OperationHandler<proto_server.Request>;
using EventHandler = shared.net.OperationHandler<proto_server.Event>;

namespace LobbyServer.controller
{
    using load_balancing;
    using shared.net.interfaces;
    using proto_server;

    public class GameNodeController : ServerController
    {
        #region Fields

        private LobbyApplication application_;
        private GameNode node_;

        #endregion

        public GameNodeController(LobbyApplication app)
            : base()
        {
            application_ = app;

            AddOperationHandler(Commands.REGISTER_GAME_NODE, new OperationHandler(HandleRegisterGameNode));

            AddEventHandler(Events.NODE_STATUS, new EventHandler(HandleNodeStatus));
        }


        #region Public Methods

        public void Disconnect()
        {
            Connection.Disconnect();
        }

        #endregion

        #region Request handlers

        private void HandleRegisterGameNode(Request request)
        {
            RegisterGameNode.Request req = request.Extract<RegisterGameNode.Request>(Commands.REGISTER_GAME_NODE);
            GameNode node = new GameNode(req.id, req.ip);
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

        #endregion
    }
}
