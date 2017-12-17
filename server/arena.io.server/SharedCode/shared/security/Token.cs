
namespace shared.security
{
    public class Token
    {
        internal Token(string value, long expiryDateMs)
        {
            Value = value;
            ExpiryDateMs = expiryDateMs;
        }

        public string Value { get; private set; }

        public long ExpiryDateMs { get; private set; }

        public override string ToString()
        {
            return Value + ":" + ExpiryDateMs.ToString();
        }
    }
}
