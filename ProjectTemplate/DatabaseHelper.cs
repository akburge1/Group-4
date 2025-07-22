// DatabaseHelper.cs  (NEW FILE)
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace ProjectTemplate
{
    /// <summary>
    ///  Centralised helper for DB operations that are *not* business‑specific.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        ///  Attempts to authenticate a user by comparing a SHA‑256 hash of the supplied
        ///  password to the stored hash.  Returns true on success.
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
                @"SELECT id, password_hash, is_admin
				  FROM   users
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

                    string storedHash = rdr.GetString("password_hash");
                    string provided = HashPasswordSHA256(password);

                    if (!String.Equals(storedHash, provided, StringComparison.OrdinalIgnoreCase))
                        return false;  // password mismatch

                    userId = rdr.GetInt32("id");
                    isAdmin = rdr.GetBoolean("is_admin");
                    return true;
                }
            }
        }

        /// <summary>
        ///  Simple SHA‑256 hash helper.  In production, use PBKDF2, bcrypt, or argon2;
        ///  but this keeps parity with the course template.
        /// </summary>
        private static string HashPasswordSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder(64);
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
