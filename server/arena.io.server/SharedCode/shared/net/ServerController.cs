using System;
using System.Collections.Generic;

using ExitGames.Concurrency.Fibers;
using shared.net.interfaces;

using Request = proto_server.Request;
using Response = proto_server.Response;
using Event = proto_server.Event;
using Commands = proto_server.Commands;
using Events = proto_server.Events;

namespace shared.net
{
    using RequestHandlerType = OperationHandler<Request>;
    using HandlersArray = List<OperationHandler<Request>>;
    using ResponseHandlerType = OperationHandler<Response>;
    using EventHandlerType = OperationHandler<Event>;

    public class ServerController
    {
        protected ServerConnection connection_;
        //Stored current request id. Warning will work only if any callback based code will be called synced on calling thread!
        protected int processedRequestId_ = -1;
        private Dictionary<int, ResponseHandlerType> pendingRequests_ = new Dictionary<int, ResponseHandlerType>();
        private Dictionary<Commands, HandlersArray> opHandlers_ = new Dictionary<Commands, HandlersArray>();
        private Dictionary<Events, EventHandlerType> eventHandlers_ = new Dictionary<Events, EventHandlerType>();

        public ServerController(ServerConnection conn)
        {
            connection_ = conn;
        }

        public ClientState State { get; private set; }

#region Public Methods
        public void SendRequest(Request request, object data = null, int error = 0)
        {
            SendRequest(request.type, data, request.id, error);
        }

        public void SendRequest(Commands cmd, object data = null, int id = 0, int error = 0)
        {
            var request = new Request();
            request.type = cmd;
            request.id = id;
            if (data != null)
            {
                ProtoBuf.Extensible.AppendValue(request, (int)cmd, data);
            }
            connection_.Send(request);
        }

        public void SendResponse(Request request, object data = null, ResponseHandlerType responseCallback = null, int error = 0)
        {
            SendResponse(request.type, data, request.id, responseCallback, error);
        }

        public void SendResponse(Commands cmd, object data = null, int id = 0, ResponseHandlerType responseCallback = null, int error = 0)
        {
            var response = new Response();
            response.type = cmd;
            response.id = id;
            response.error = error;
            response.timestamp = helpers.CurrentTime.Instance.CurrentTimeInMs;
            if (data != null)
            {
                ProtoBuf.Extensible.AppendValue(response, (int)cmd, data);
            }
            connection_.Send(response);

            if (responseCallback != null)
                pendingRequests_.Add(id, responseCallback);
        }

        public void SendEvent(Events evtCode, object data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            var evt = new Event();
            evt.type = evtCode;
            evt.timestamp = helpers.CurrentTime.Instance.CurrentTimeInMs;

            ProtoBuf.Extensible.AppendValue(evt, (int)evtCode, data);
            Send(evt);
        }

        public virtual void HandleDisconnect()
        {
        }

        public virtual bool FilterRequest(Request request)
        {
            return true;
        }

        public virtual bool FilterEvent(Event evt)
        {
            return true;
        }

        public void AddOperationHandler(Commands cmd, RequestHandlerType handler)
        {
            if (!opHandlers_.ContainsKey(cmd))
            {
                opHandlers_.Add(cmd, new HandlersArray());
            }
            HandlersArray handlersForCmd;
            if (opHandlers_.TryGetValue(cmd, out handlersForCmd))
            {
                handlersForCmd.Add(handler);
            }
        }

        public void AddEventHandler(Events evt, EventHandlerType handler)
        {
            eventHandlers_[evt] = handler;
        }

        public void HandleRequest(Request request)
        {
            HandleRequestInternal(request);
        }

        public void HandleResponse(Response response)
        {
            ResponseHandlerType handler;
            if (pendingRequests_.TryGetValue(response.id, out handler))
            {
                handler.TryExecute(response);          
            }
        }

        public void HandleEvent(Event evt)
        {
            HandleEventInternal(evt);
        }
#endregion

#region Private Methods
        private void HandleEventInternal(Event evt)
        { 
            if (FilterEvent(evt))
            {
                return;
            }

            EventHandlerType handler;
            if (eventHandlers_.TryGetValue(evt.type, out handler))
            {
                handler.TryExecute(evt);
            }
        }

        private void HandleRequestInternal(Request request)
        {
            //If filtering successed, then discard this request
            if (FilterRequest(request))
            {
                //("Request(type:" + request.type + ", id:" + request.id + ") failed condition check");
                return;
            }

            HandlersArray handlersForCmd;
            //if there is any operation handler then execute it, otherwise chain it to nested handlers
            if (opHandlers_.TryGetValue(request.type, out handlersForCmd))
            {
                processedRequestId_ = request.id;
                foreach (var handler in handlersForCmd)
                {
                    handler.TryExecute(request);
                }

                return;
            }

            return;
        }

        private void Send(Event evt)
        {
            if (connection_ != null)
            {
                connection_.Send(evt);
            }
        }

        protected virtual IActionInvoker GetActionInvoker()
        {
            return null;
        }
#endregion
    }
}
