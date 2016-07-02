using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database.Postgres
{
    class PostgresImpl : IDatabaseImpl
    {
        private AuthDB authDB_ = new AuthDB();
        private PlayerDB playerDB_ = new PlayerDB();

        IAuthDB IDatabaseImpl.AuthDB
        {
            get { return authDB_; }
        }

        IPlayerDB IDatabaseImpl.PlayerDB
        {
            get { return playerDB_; }
        }

        ILootDB IDatabaseImpl.LootDB
        {
            get { throw new NotImplementedException(); }
        }

        ICreatureDB IDatabaseImpl.CreatureDB
        {
            get { throw new NotImplementedException(); }
        }

        IWorldDB IDatabaseImpl.WorldDB
        {
            get { throw new NotImplementedException(); }
        }

        ISpellDB IDatabaseImpl.SpellDB
        {
            get { throw new NotImplementedException(); }
        }

        string[] IDatabaseImpl.ExtractStringArray(object arrValue)
        {
            throw new NotImplementedException();
        }
    }
}
