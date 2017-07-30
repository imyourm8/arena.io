using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace shared.database.Postgres
{
    public class WorldDB : IWorldDB
    {
        void IWorldDB.GetExpBlocks(QueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM exp_blocks;", conn))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader != null)
                    {
                        cb(QueryResult.Success, reader);
                    }
                }
            }
        }

        void IWorldDB.GetBoosters(QueryCallback cb)
        {
            return;
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM boosters;", conn))
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
