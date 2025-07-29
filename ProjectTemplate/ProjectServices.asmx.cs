// ProjectServices.asmx.cs
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;

namespace ProjectTemplate
{
    public class PromptInfo
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class ProjectServices : System.Web.Services.WebService
    {
        // ─────────────────────────────────────────────────────────────────────
        // DATABASE CREDENTIALS – keep these in sync with your MySQL instance
        private string dbID = "cis440summer2025team4";
        private string dbPass = "cis440summer2025team4";
        private string dbName = "cis440summer2025team4";
        // ─────────────────────────────────────────────────────────────────────

        private string getConString()
        {
            return $"SERVER=107.180.1.16;PORT=3306;DATABASE={dbName};UID={dbID};PASSWORD={dbPass}";
        }

        // --------------------------------------------------------------------
        // EXISTING CONNECTIVITY TEST – unchanged
        [WebMethod(EnableSession = true)]
        public string TestConnection()
        {
            try
            {
                string testQuery = "SELECT 1";
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    MySqlCommand cmd = new MySqlCommand(testQuery, con);
                    con.Open();
                    cmd.ExecuteScalar();
                }
                return "Success!";
            }
            catch (Exception e)
            {
                return "Connection failed: " + e.Message;
            }
        }

        // --------------------------------------------------------------------
        // NEW LOGIN END‑POINT
        // Returns   "admin"     → admin user
        //           "employee"  → standard user
        //           "invalid"   → credentials rejected
        [WebMethod(EnableSession = true)]
        public string Login(string username, string password)
        {
            bool isAdmin;
            int userId;

            if (DatabaseHelper.TryAuthenticate(getConString(), username, password, out isAdmin, out userId))
            {
                // Persist minimal session data
                Session["userID"] = userId;
                Session["isAdmin"] = isAdmin;
                Session.Timeout = 30;          // minutes

                return isAdmin ? "admin" : "employee";
            }

            return "invalid";
        }

        [WebMethod(EnableSession = true)]
        public PromptInfo GetCurrentPrompt()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();
                    string query = "SELECT id, question_text FROM Prompts ORDER BY date_posted DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                return new PromptInfo
                                {
                                    Id = rdr.GetInt32("id"),
                                    Text = rdr.GetString("question_text")
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [WebMethod(EnableSession = true)]
        public string SubmitFeedback(string message, bool isAnonymous)
        {
            if (Session["userID"] == null)
            {
                return "not_authenticated";
            }

            int userId = (int)Session["userID"];

            try
            {
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();

                    // Get current prompt id
                    string promptQuery = "SELECT id FROM Prompts ORDER BY date_posted DESC LIMIT 1";
                    int? promptId = null;
                    using (MySqlCommand cmd = new MySqlCommand(promptQuery, con))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            promptId = Convert.ToInt32(result);
                        }
                    }

                    if (promptId == null)
                    {
                        return "no_current_prompt";
                    }

                    // Check if already submitted
                    string checkQuery = "SELECT COUNT(*) FROM Feedback WHERE user_id = @userId AND prompt_id = @promptId";
                    using (MySqlCommand cmd = new MySqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@promptId", promptId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            return "already_submitted";
                        }
                    }

                    // Insert
                    string insertQuery = @"INSERT INTO Feedback (prompt_id, user_id, message, is_anonymous)
                                           VALUES (@promptId, @userId, @message, @isAnonymous)";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@promptId", promptId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.Parameters.AddWithValue("@isAnonymous", isAnonymous);
                        cmd.ExecuteNonQuery();
                    }

                    return "success";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }
    }
}