using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.net.interfaces
{
    public interface IBaseController
    { 
        
    }

    public interface IController<TConnection> : IBaseController
        where TConnection : IBaseConnection
    {
        TConnection Connection { set; }
    }
}
