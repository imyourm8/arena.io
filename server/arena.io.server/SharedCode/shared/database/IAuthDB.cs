using System.Data;
using System.Threading.Tasks;

namespace shared.database
{
    public class AuthEntry
    {
        public string authUserID;
    }

    public interface IAuthDB
    {        
        Task<IDataReader> LoginUser(AuthEntry authEntry);
        void SetLoginToken(string userID, string token, long tokenExpiryDate, NonQueryCallback cb);
        void LoginUserByNickname(string name, QueryCallback cb);
        Task<IDataReader> CreateUser(AuthEntry authEntry);
        void CreateUserWithNickname(string name, QueryCallback cb);
    }
}
