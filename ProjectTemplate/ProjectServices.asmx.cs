// ProjectServices.asmx.cs
using System;
using System.Data;
using System.Web.Services;
using MySql.Data.MySqlClient;

namespace ProjectTemplate
{
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
    }
}