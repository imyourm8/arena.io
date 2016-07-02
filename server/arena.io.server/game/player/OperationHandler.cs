using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.player
{
    using RequestDataType = proto_common.Request;
    using RequestCmdType = proto_common.Commands;
    public class OperationHandler
    {
        public delegate void HandlerDelegate(RequestDataType request);

        private HandlerDelegate delegate_;

        public OperationHandler(HandlerDelegate func)
        {
            delegate_ = func;
        }

        public void TryExecute(RequestDataType request)
        {
            if (delegate_ != null)
            {
                delegate_(request);
            }
        }
    }
}
