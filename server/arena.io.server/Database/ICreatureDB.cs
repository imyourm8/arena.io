using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    interface ICreatureDB
    {
        void GetAll(Database.QueryCallback cb); 
    }
}
