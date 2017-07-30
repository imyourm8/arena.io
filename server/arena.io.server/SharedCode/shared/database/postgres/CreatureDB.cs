using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace shared.database.Postgres
{
    public class CreatureDB : ICreatureDB
    {
        void ICreatureDB.GetAll(QueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM mobs;", conn))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader != null)
                    {
                        cb(QueryResult.Success, reader);
                    }
                }
            }
        }
    }
}
