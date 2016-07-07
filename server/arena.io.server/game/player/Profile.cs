using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace arena.player
{
    class Profile : PlayerExperience.IExpProvider
    {
        private PlayerProfileExperience exp_;
        private HashSet<proto_profile.PlayerClasses> unlockedClasses_ = new HashSet<proto_profile.PlayerClasses>();

        public Profile(IDataReader data)
        {
            exp_ = new PlayerProfileExperience(this);

            Exp = (int)data["exp"];
            Level = (int)data["level"];
            Coins = (int)data["coins"];
            Name = (string)data["name"];
            UniqueID = (string)data["authUserID"];

            var reader = new JsonTextReader(new StringReader((string)data["unlocked_classes"]));
            HashSet<proto_profile.PlayerClasses> unclockedClasses = new HashSet<proto_profile.PlayerClasses>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.EndArray) continue;
                //json looks like array of classes ids
                unclockedClasses.Add(helpers.Parsing.ParseEnum<proto_profile.PlayerClasses>((string)reader.Value));
            }
        }

        public string GetSerializedUnlockedClasses()
        {
            return JsonConvert.SerializeObject(unlockedClasses_);
        }

        public HashSet<proto_profile.PlayerClasses> GetUnlockedClasses()
        {
            return unlockedClasses_;
        }

        public string UniqueID
        { get; private set; }

        public int Coins
        { get; private set; }

        public int Exp
        { get; private set; }

        public int Level
        { get; private set; }

        public string Name
        { get; private set; }

        public void AddExperience(int points)
        {
            exp_.AddExperience(points);
        }

        public void AddCoins(int coins)
        {
            Coins += coins;
        }

        public bool TryWithdrawCoins(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            return true;
        }

        public proto_profile.UserInfo GetInfoPacket()
        {
            proto_profile.UserInfo info = new proto_profile.UserInfo();
            info.coins = Coins;
            info.level = Level;
            info.name = Name;
            return info;
        }

        int Experience.IExpProvider.Exp
        {
            get
            {
                return Exp;
            }
            set
            {
                Exp = value;
            }
        }

        int Experience.IExpProvider.Level
        {
            get
            {
                return Level;
            }
            set
            {
                Level = value;
            }
        }
    }
}
