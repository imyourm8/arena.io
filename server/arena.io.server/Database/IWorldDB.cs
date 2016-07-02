using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    interface IWorldDB
    {
        void GetGroupEvents(Database.QueryCallback cb);
    }
}
