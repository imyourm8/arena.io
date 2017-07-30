using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.database.Postgres
{
    public class PostgresImpl : IDatabaseImpl
    {
        private AuthDB authDB_ = new AuthDB();
        private PlayerDB playerDB_ = new PlayerDB();
        private CreatureDB creatureDB_ = new CreatureDB();
        private WorldDB worldDB_ = new WorldDB();

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
            get { return creatureDB_; }
        }

        IWorldDB IDatabaseImpl.WorldDB
        {
            get { return worldDB_; }
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
