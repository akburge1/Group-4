// DatabaseHelper.cs
using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProjectTemplate
{
    /// <summary>
    /// Centralized helper for DB operations that are *not* business-specific.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Attempts to authenticate a user by comparing the supplied plain text password
        /// to the stored password. Returns true on success.
        /// </summary>
        public static bool TryAuthenticate(
            string connectionString,
            string username,
            string password,
            out bool isAdmin,
            out int userId)
        {
            isAdmin = false;
            userId = 0;

            const string sql =
                @"SELECT id, password, is_admin
                  FROM   Users
                  WHERE  username = @un
                  LIMIT  1";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@un", username);
                con.Open();

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                        return false;  // username not found

                    string storedPassword = rdr.GetString("password");

                    if (!String.Equals(storedPassword, password, StringComparison.OrdinalIgnoreCase))
                        return false;  // password mismatch

                    userId = rdr.GetInt32("id");
                    isAdmin = rdr.GetBoolean("is_admin");
                    return true;
                }
            }
        }
    }
}