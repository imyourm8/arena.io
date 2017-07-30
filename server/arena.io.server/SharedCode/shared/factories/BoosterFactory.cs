using System;
using System.IO;
using System.Collections.Generic;
using System.Data;

using shared.data;
using shared.database;

namespace shared.factories
{
    public class BoosterFactory : Singleton<BoosterFactory>
    {
        private Dictionary<proto_game.BoosterType, BoosterEntry> boosters_ = new Dictionary<proto_game.BoosterType, BoosterEntry>();

        public void Init()
        {
            database.Database.Instance.GetWorldDB().GetBoosters((QueryResult result, IDataReader data) =>
                {
                    if (result == QueryResult.Success)
                    {
                        while (data.Read())
                        {
                            var entry = new BoosterEntry(data);
                            boosters_.Add(entry.Type, entry);
                        }
                    }
                });
        }

        public BoosterEntry GetEntry(proto_game.BoosterType type)
        {
            return boosters_[type];
        }
    }
}
