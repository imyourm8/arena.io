using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    interface ILootDB
    {
        void GetLootByID(long lootID, Database.QueryCallback cb);
        void GetLootByRequiredLevel(long level, Database.QueryCallback cb);
    }
}
