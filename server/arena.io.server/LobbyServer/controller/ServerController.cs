using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;

using System.Collections.Generic;

namespace LobbyServer.controller
{
    public class ServerController : OutboundS2SPeer
    {
        public ServerController(ApplicationBase application)
            : base(application)
        {
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            var parameters = new Dictionary<byte, object>();
            parameters[0] = "";
            SendOperationRequest(new OperationRequest 
            { 
                OperationCode = 1, Parameters = parameters 
            }, new SendParameters());
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            
        }
    }
}
