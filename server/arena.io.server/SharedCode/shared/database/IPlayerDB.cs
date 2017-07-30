using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace shared.database
{
    public class ProfileData
    {
        public string Name { get; set; }
        public string UniqueID { get; set; }
        public int Coins { get; set; }
        public int Exp { get; set; }
        public int Level { get; set; }
    }

    public interface IPlayerDB
    {
        void GetClasses(database.QueryCallback cb);
        void ChangeNickname(string userID, string name, database.NonQueryCallback cb);
        void SaveProfile(ProfileData profile, database.NonQueryCallback cb);
    }
}
