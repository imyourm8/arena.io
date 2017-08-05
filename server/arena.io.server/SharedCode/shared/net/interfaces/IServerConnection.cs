using Photon.SocketServer;

using Response = proto_server.Response;
using Request = proto_server.Request;
using Event = proto_server.Event;

namespace shared.net.interfaces
{
    public interface IServerConnection<TController> : IConnection<TController>
        where TController : IBaseController
    {
        void Send(Response response);
        void Send(Response response, SendParameters sendParameters);

        void Send(Request request);
        void Send(Request request, SendParameters sendParameters);

        void Send(Event evt);
        void Send(Event evt, SendParameters sendParameters);
    }
}
