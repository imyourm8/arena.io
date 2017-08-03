using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.net.interfaces
{
    public interface IRequestFilter
    {
        bool FilterRequest(proto_common.Request request);
    }
}
