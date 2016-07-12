using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace arena.Database.Postgres
{
    class AuthDB : IAuthDB
    {
        async void IAuthDB.LoginUser(AuthEntry authEntry, Database.QueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM users WHERE \"authUserID\" = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", authEntry.authUserID);
                    var task = cmd.ExecuteReaderAsync();
                    var reader = await task;

                    if (reader != null)
                    {
                        cb(QueryResult.Success, reader);
                    }
                }
            }
        }

        async void IAuthDB.CreateUser(AuthEntry authEntry, Database.QueryCallback cb)
        {
            int result = 0;

            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO users (\"authUserID\") VALUES (@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", authEntry.authUserID);
                    result = await cmd.ExecuteNonQueryAsync();
                }
            }

            if (result == 1)
            {
                (this as IAuthDB).LoginUser(authEntry, cb);
            }
        }


        void IAuthDB.LoginUserByNickname(string name, Database.QueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM users WHERE name = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    var reader = cmd.ExecuteReader();

                    if (reader != null)
                    {
                        cb(QueryResult.Success, reader);
                    }
                }
            }
        }


        void IAuthDB.CreateUserWithNickname(string name, Database.QueryCallback cb)
        {
            int result = 0;

            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO users (name) VALUES (@name)", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    result = cmd.ExecuteNonQuery();
                }
            }

            if (result == 1)
            {
                (this as IAuthDB).LoginUserByNickname(name, cb);
            }
        }
    }
}
