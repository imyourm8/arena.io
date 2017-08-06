using System;

using Photon.SocketServer;
using Photon.SocketServer.ServerToServer;

namespace shared.net
{
    using interfaces;

    public class OutboundServerConection : OutboundS2SPeer, IServerConnection
    {
        private ServerConnection connection_;

        public OutboundServerConection(ServerApplication app)
            : base(app)
        {
            connection_ = new ServerConnection(app, this);
        }

        public ServerApplication Application
        {
            get { return connection_.Application; }
        }

        public void Send(proto_server.Response response)
        {
            connection_.Send(response);
        }

        public void Send(proto_server.Response response, Photon.SocketServer.SendParameters sendParameters)
        {
            connection_.Send(response, sendParameters);
        }

        public void Send(proto_server.Request request)
        {
            connection_.Send(request);
        }

        public void Send(proto_server.Request request, Photon.SocketServer.SendParameters sendParameters)
        {
            connection_.Send(request, sendParameters);
        }

        public void Send(proto_server.Event evt)
        {
            connection_.Send(evt);
        }

        public void Send(proto_server.Event evt, Photon.SocketServer.SendParameters sendParameters)
        {
            connection_.Send(evt, sendParameters);
        }

        public void SetController(ServerController controller)
        {
            connection_.SetController(controller);
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            connection_.OnConnectionEstablished(responseObject);
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            connection_.OnEvent(eventData, sendParameters);
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            connection_.OnOperationResponse(operationResponse, sendParameters);
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            connection_.OnDisconnect(reasonCode, reasonDetail);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            connection_.OnOperationRequest(operationRequest, sendParameters);
        }
    }
}
