using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace arena.Database
{
    interface IPlayerDB
    {
        void GetClasses(Database.QueryCallback cb);
        void ChangeNickname(string userID, string name, Database.NonQueryCallback cb);
        void SaveProfile(player.Profile profile, Database.NonQueryCallback cb);
    }
}
