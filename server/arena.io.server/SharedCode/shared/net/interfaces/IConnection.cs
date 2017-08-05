using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.net.interfaces
{
    public interface IBaseConnection
    { 
        
    }

    public interface IConnection<TController> : IBaseConnection
        where TController : IBaseController
    {
        void SetController(TController controller);
    }
}
