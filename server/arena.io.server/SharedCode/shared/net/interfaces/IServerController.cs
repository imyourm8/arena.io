using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using Commands = proto_server.Commands;
using Events = proto_server.Events;

namespace shared.net.interfaces
{
    using RequestHandlerType = OperationHandler<Request>;
    using ResponseHandlerType = OperationHandler<Response>;
    using EventHandlerType = OperationHandler<Event>;

    public interface IServerController<TConnection> : IController<TConnection>
        where TConnection : IServerConnection
    {
        void SendRequest(Commands cmd, object data, int error);
        void SendResponse(Request request, object data, ResponseHandlerType responseCallback, int error);
        void SendResponse(Commands cmd, object data, int id, ResponseHandlerType responseCallback, int error);
        void SendEvent(Events evtCode, object data);
        void Send(Event evt);

        void HandleRequest(Request request);
        void HandleResponse(Response response);
        void HandleEvent(Event evt);
        void HandleDisconnect();
        /*void HandleEventInternal(Event evt);
        void HandleRequestInternal(Request request);*/
    }
}
