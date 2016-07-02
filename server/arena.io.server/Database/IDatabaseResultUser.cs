using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace arena.Database
{
    interface IDatabaseResultUser
    {
        void ReadFrom(IDataReader data);
    }
}
