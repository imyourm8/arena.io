using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    public enum QueryResult
    {
        Success,
        Fail
    };

    class Database : TapCommon.Singleton<Database>
    {
        public delegate void QueryCallback(QueryResult result, IDataReader data);
        public delegate void NonQueryCallback(QueryResult result);

        private IDatabaseImpl databaseImpl_;

        public void SetDatabaseImplementation(IDatabaseImpl impl)
        {
            databaseImpl_ = impl;
        }

        public IAuthDB GetAuthDB()
        {
            return databaseImpl_.AuthDB;
        }

        public IPlayerDB GetPLayerDB()
        {
            return databaseImpl_.PlayerDB; 
        }

        public ILootDB GetLootDB()
        {
            return databaseImpl_.LootDB;
        }

        public ICreatureDB GetCreatureDB()
        {
            return databaseImpl_.CreatureDB;
        }

        public ISpellDB GetSpellDB()
        {
            return databaseImpl_.SpellDB;
        }

        public IWorldDB GetWorldDB()
        {
            return databaseImpl_.WorldDB;
        }

        public string[] ExtractStringArray(object arrValue)
        {
            return databaseImpl_.ExtractStringArray(arrValue);
        }
    }
}
