using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.database
{
    public interface IDatabaseSerializable
    {
        void SaveToDatabase();
        void LoadFromDatabase();
    }
}
