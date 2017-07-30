using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace shared.database
{
    public interface IDatabaseResultUser
    {
        void ReadFrom(IDataReader data);
    }
}
