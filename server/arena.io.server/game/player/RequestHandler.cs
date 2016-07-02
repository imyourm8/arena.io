using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Concurrency.Fibers;

namespace arena.player
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
        }

        protected virtual IActionInvoker GetActionInvoker()
        {
            return null;
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

        private bool IsNested()
        {
            return parent_ != null;
        }

        private void RemoveNestedHandler(RequestHandler handler)
        {
            nestedHandlers_.Remove(handler);
            handler.parent_ = null;
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
            bool result = HandleRequestInternal(request);
        }

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
    }
}
