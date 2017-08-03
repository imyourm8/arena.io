using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.net
{
    public class OperationHandler<TRequest>
    {
        public delegate void HandlerDelegate(TRequest request);

        private HandlerDelegate delegate_;

        public OperationHandler(HandlerDelegate func)
        {
            delegate_ = func;
        }

        public void TryExecute(TRequest request)
        {
            if (delegate_ != null)
            {
                delegate_(request);
            }
        }
    }
}
