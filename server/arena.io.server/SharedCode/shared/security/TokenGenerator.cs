using System;
using System.Text;
using System.Security.Cryptography;

namespace shared.security
{
    public class TokenGenerator
    {
        private long tokenLifeTime_;
        public TokenGenerator(long tokenLifeTime)
        {
            tokenLifeTime_ = tokenLifeTime;
        }

        public Token Generate(long currentTimeMs)
        {
            var localTime = 1;
            var token = localTime + Guid.NewGuid().ToString();

            var crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(token), 0, Encoding.UTF8.GetByteCount(token));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return new Token(hash.ToString(), currentTimeMs + tokenLifeTime_);
        }
    }
}
