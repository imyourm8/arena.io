using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    public class AuthEntry
    {
        public string authUserID;
    }

    interface IAuthDB
    {        
        void LoginUser(AuthEntry authEntry, Database.QueryCallback cb);
        void LoginUserByNickname(string authEntry, Database.QueryCallback cb);
        void CreateUser(AuthEntry authEntry, Database.QueryCallback cb);
        void CreateUserWithNickname(string name, Database.QueryCallback cb);
    }
}
