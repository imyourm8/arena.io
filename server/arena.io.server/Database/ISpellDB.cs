using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    interface ISpellDB
    {
        void LoadAllSpellData(Database.QueryCallback cb);
        void LoadAllSpellEffectsData(Database.QueryCallback cb);
        void LoadAllStatusEffectsData(Database.QueryCallback cb);
    }
}
