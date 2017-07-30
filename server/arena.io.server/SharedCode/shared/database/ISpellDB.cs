using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.database
{
    public interface ISpellDB
    {
        void LoadAllSpellData(database.QueryCallback cb);
        void LoadAllSpellEffectsData(database.QueryCallback cb);
        void LoadAllStatusEffectsData(database.QueryCallback cb);
    }
}
