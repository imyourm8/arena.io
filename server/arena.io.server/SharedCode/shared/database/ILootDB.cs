using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.database
{
    public interface ILootDB
    {
        void GetLootByID(long lootID, database.QueryCallback cb);
        void GetLootByRequiredLevel(long level, database.QueryCallback cb);
    }
}
