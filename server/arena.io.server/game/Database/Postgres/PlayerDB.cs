﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace arena.Database.Postgres
{
    class PlayerDB : IPlayerDB
    {
        void IPlayerDB.GetClasses(Database.QueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM classes;", conn))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader != null)
                    {
                        cb(QueryResult.Success, reader);
                    }
                }
            }
        }


        async void IPlayerDB.ChangeNickname(string userID, string name, Database.NonQueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE users SET name = @name WHERE \"authUserID\" = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("id", userID);

                    var result = await cmd.ExecuteNonQueryAsync();
                    cb(result == 1 ? QueryResult.Success : QueryResult.Fail);
                }
            }
        }


        async void IPlayerDB.SaveProfile(player.Profile profile, Database.NonQueryCallback cb)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnectionDefines.PostgresParams))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE users SET name=@name, coins=@coins,exp=@exp,level=@level,unlocked_classes=@uc WHERE \"authUserID\" = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("name", profile.Name);
                    cmd.Parameters.AddWithValue("id", profile.UniqueID);
                    cmd.Parameters.AddWithValue("coins", profile.Coins);
                    cmd.Parameters.AddWithValue("exp", profile.Exp);
                    cmd.Parameters.AddWithValue("level", profile.Level);
                    cmd.Parameters.AddWithValue("uc", profile.GetSerializedUnlockedClasses());

                    var result = await cmd.ExecuteNonQueryAsync();
                    cb(result == 1 ? QueryResult.Success : QueryResult.Fail);
                }
            }
        }
    }
}
