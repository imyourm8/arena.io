using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Concurrency.Fibers;

namespace shared.net
{
    using RequestDataType = proto_common.Request;
    using RequestCommandType = proto_common.Commands;
    using HandlerType = OperationHandler;
    using HandlersArray = List<OperationHandler>;

    public interface IActionInvoker
    {
        void Execute(Action action);
    }

    public interface IRequestFilter
    {
        bool FilterRequest(RequestDataType request);
    }

    public class RequestHandler : IRequestFilter
    {
        protected IGameConnection connection_;
        //Stored current request id. Warning will work only if any callback based code will be called synced on calling thread!
        protected int processedRequestId_ = -1;
        private RequestHandler parent_ = null;
        private Dictionary<RequestCommandType, HandlersArray> opHandlers_
            = new Dictionary<RequestCommandType, HandlersArray>();
        private List<RequestHandler> nestedHandlers_ = new List<RequestHandler>();

        public IGameConnection Connection
        {
            set { connection_ = value; }
            protected get { return connection_; }
        }

#region Public Methods
        public void SendResponse(proto_common.Request request, object data = null, int error = 0)
        {
            SendResponse(request.type, data, request.id, error);
        }

        public void SendResponse(proto_common.Commands cmd, object data = null, int id = 0, int error = 0)
        {
            var response = new proto_common.Response();
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

        public void SendEvent(proto_common.Events evtCode, object data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            var evt = new proto_common.Event();
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

        public virtual bool FilterRequest(RequestDataType request)
        {
            return true;
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

        public void AddOperationHandler(RequestCommandType cmd, HandlerType handler)
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

        public void HandleRequest(RequestDataType request)
        {
            GetActionInvoker().Execute(() =>
            {
                bool result = HandleRequestInternal(request);
            });
        }
#endregion

#region Public Methods
        private bool HandleRequestInternal(RequestDataType request)
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
