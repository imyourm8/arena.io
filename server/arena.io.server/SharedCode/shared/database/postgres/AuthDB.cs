using System.Data;
using System.Threading.Tasks;

using Npgsql;

namespace shared.database.Postgres
{
    public class AuthDB : IAuthDB
    {
        async Task<IDataReader> IAuthDB.LoginUser(AuthEntry authEntry)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM users WHERE \"authUserID\" = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", authEntry.authUserID);
                    try
                    {
                        var task = cmd.ExecuteReaderAsync();
                        var reader = await task;
                        return reader;
                    }
                    catch (System.Data.Common.DbException e)
                    {
                        return null;
                    }
                }
            }
        }

        async Task<IDataReader> IAuthDB.CreateUser(AuthEntry authEntry)
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
                try
                {
                    return await (this as IAuthDB).LoginUser(authEntry);
                }
                catch (System.Data.Common.DbException e) 
                {
                    return null;
                }
            }

            return null;
        }


        void IAuthDB.LoginUserByNickname(string name, QueryCallback cb)
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

        async void IAuthDB.CreateUserWithNickname(string name, QueryCallback cb)
        {
            int result = 0;

            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO users (name) VALUES (@name)", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    result = await cmd.ExecuteNonQueryAsync();
                }
            }

            if (result == 1)
            {
                (this as IAuthDB).LoginUserByNickname(name, cb);
            }
        }

        async void IAuthDB.SetLoginToken(string id, string token, NonQueryCallback cb)
        {
            int result = 0;

            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("UPDATE users (login_token) VALUES (@login_token) WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("login_token", token);
                    cmd.Parameters.AddWithValue("id", id);
                    result = await cmd.ExecuteNonQueryAsync();
                    cb(QueryResult.Success);
                }
            }
        }

        async void IAuthDB.SetGameToken(string userID, string token, NonQueryCallback cb)
        {
            int result = 0;

            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("UPDATE users (game_token) VALUES (@game_token) WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("game_token", token);
                    cmd.Parameters.AddWithValue("id", userID);
                    result = await cmd.ExecuteNonQueryAsync();
                    cb(QueryResult.Success);
                }
            }
        }
    }
}
