using Photon.SocketServer;

namespace shared.net.interfaces
{
    public interface IServerPeerResponder
    {
        void OnEvent(IEventData eventData, SendParameters sendParameters);
        void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters);
        void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail);
        void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters);
    }
}
