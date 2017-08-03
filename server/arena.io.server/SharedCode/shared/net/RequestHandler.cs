using System;
using System.Collections.Generic;

using ExitGames.Concurrency.Fibers;

using shared.net.interfaces;

using Request = proto_common.Request;
using Response = proto_common.Response;
using Event = proto_common.Event;
using Commands = proto_common.Commands;
using Events = proto_common.Events;

namespace shared.net
{
    using HandlerType = OperationHandler<Request>;
    using HandlersArray = List<OperationHandler<Request>>;

    public class RequestHandler : IRequestFilter
    {
        // Store current request id. Warning will work only if any callback based code will be called synced on calling thread!
        protected int processedRequestId_ = -1;
        protected IGameConnection connection_;
        private RequestHandler parent_ = null;
        private Dictionary<Commands, HandlersArray> opHandlers_ = new Dictionary<Commands, HandlersArray>();
        private List<RequestHandler> nestedHandlers_ = new List<RequestHandler>();

        public RequestHandler()
        {
            State = ClientState.NotLogged;
        }

        public IGameConnection Connection
        {
            set { connection_ = value; }
            protected get { return connection_; }
        }

        public ClientState State { get; private set; }

#region Public Methods
        public void SetState(ClientState state)
        {
            State |= state;
        }

        public void RemoveState(ClientState state)
        {
            State &= ~state;
        }

        public void ResetState(ClientState state)
        {
            State = state;
        }

        public void SendResponse(Request request, object data = null, int error = 0)
        {
            SendResponse(request.type, data, request.id, error);
        }

        public void SendResponse(Commands cmd, object data = null, int id = 0, int error = 0)
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
            foreach (var handler in nestedHandlers_)
            {
                handler.HandleDisconnect();
            }
        }

        public virtual bool FilterRequest(Request request)
        {
            bool filtered = false;
            bool alwaysExecute = false;
            OperationCondition condition;
            if (OperationCondition.conditionList.TryGetValue((int)request.type, out condition))
            {
                //iterate through every possible state
                foreach (ClientState state in Enum.GetValues(typeof(ClientState)))
                {
                    //if state required in condition call 
                    if ((condition.State & state) != 0)
                    {
                        //and exists in current state
                        if ((State & state) == 0)
                        {
                            filtered |= true;
                        }
                    }
                }

                if (condition.Execution == OperationCondition.ExecutionMethod.AlwaysExecute)
                {
                    alwaysExecute = true;
                }
            }

            return filtered && !alwaysExecute;
        }

        public void AddNestedRequestHandler(RequestHandler requestHandler)
        {
            if (requestHandler.IsNested())
            {
                throw new ArgumentException("Trying to add nested RequestHandler which already has been added somewhere else");
            }
            nestedHandlers_.Add(requestHandler);
            requestHandler.parent_ = this;
        }

        public void DetachFromParentHandler()
        {
            if (IsNested())
            {
                RemoveNestedHandler(this);
            }
        }

        public void AddOperationHandler(Commands cmd, HandlerType handler)
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

        public void HandleRequest(Request request)
        {
            GetActionInvoker().Execute(() =>
            {
                bool result = HandleRequestInternal(request);
            });
        }
#endregion

#region Public Methods
        private bool HandleRequestInternal(Request request)
        {
            //If filtering successed, then discard this request
            if (FilterRequest(request))
            {
                //("Request(type:" + request.type + ", id:" + request.id + ") failed condition check");
                return false;
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

                return true;
            }
            else
            {
                foreach (var handler in nestedHandlers_)
                {
                    if (handler.HandleRequestInternal(request))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Send(proto_common.Event evt)
        {
            if (connection_ != null)
            {
                connection_.Send(evt);
            }
        }

        private bool IsNested()
        {
            return parent_ != null;
        }

        protected virtual IActionInvoker GetActionInvoker()
        {
            return null;
        }

        private void RemoveNestedHandler(RequestHandler handler)
        {
            nestedHandlers_.Remove(handler);
            handler.parent_ = null;
        }
#endregion
    }
}
