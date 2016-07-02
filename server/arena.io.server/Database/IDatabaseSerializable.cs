using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    public interface IDatabaseSerializable
    {
        void SaveToDatabase();
        void LoadFromDatabase();
    }
}
