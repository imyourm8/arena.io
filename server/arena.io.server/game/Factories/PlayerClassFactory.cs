using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using arena.battle;

namespace arena.Factories
{
    class PlayerClassFactory : TapCommon.Singleton<PlayerClassFactory>
    {
        private Dictionary<proto_profile.PlayerClasses, PlayerClassEntry> classes_ = new Dictionary<proto_profile.PlayerClasses, PlayerClassEntry>();

        public void Init()
        {
            Database.Database.Instance.GetPLayerDB().GetClasses(HandleClassesFromDB);
        }

        private void HandleClassesFromDB(Database.QueryResult result, IDataReader data)
        {
            if (result != Database.QueryResult.Success)
            {
                return;
            }

            do
            {
                var entry = new PlayerClassEntry(data);
                classes_.Add(entry.@Class, entry);
            } while (data.Read());
        }

        public PlayerClassEntry GetEntry(proto_profile.PlayerClasses cls)
        {
            return classes_[cls];
        }

        public IReadOnlyDictionary<proto_profile.PlayerClasses, PlayerClassEntry> GetAllClasses()
        {
            return classes_;
        }
    }
}
