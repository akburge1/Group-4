// ProjectServices.asmx.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Script.Services;

namespace ProjectTemplate
{
    public class PromptInfo
    {
        public int id { get; set; }
        public string text { get; set; }
        public string weekStartIso { get; set; }
    }

    public class FeedbackItem
    {
        public int id { get; set; }
        public string dateSubmitted { get; set; }   // ISO-like string
        public string displayName { get; set; }     // "Anonymous" if isAnonymous = true
        public bool isAnonymous { get; set; }
        public string message { get; set; }

        public int promptId { get; set; }
        public string promptText { get; set; }
        public string weekStartIso { get; set; }    // from Prompts.date_posted
    }

    public class FeedbackListResult
    {
        public int totalCount { get; set; }
        public List<FeedbackItem> items { get; set; }
    }

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class ProjectServices : WebService
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

        // Try to resolve the Eastern/Detroit zone even if hosted on Linux.
        private static TimeZoneInfo GetEasternTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); } // Windows
            catch
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("America/Detroit"); }   // Linux
                catch { return TimeZoneInfo.Utc; }
            }
        }

        // Compute Monday 00:00 (local Eastern) for "now".
        private static DateTime GetCurrentWeekStartEastern()
        {
            var tz = GetEasternTz();
            var nowEastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            // ISO week: Monday=1 ... Sunday=7
            int dayOfWeek = (int)nowEastern.DayOfWeek; // Sunday=0 ... Saturday=6
            int isoDow = dayOfWeek == 0 ? 7 : dayOfWeek; // map Sunday(0) -> 7
            var monday = nowEastern.Date.AddDays(-(isoDow - 1)); // back to Monday
            return new DateTime(monday.Year, monday.Month, monday.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        // Resolve the prompt for the current week.
        // Stateful: stamps the chosen prompt into Prompts.date_posted = <weekStart> and sets is_used = 1.
        // Uses a MySQL named lock so only one request stamps per week.
        private PromptInfo ResolveOrStampWeeklyPrompt(MySqlConnection con)
        {
            DateTime weekStartLocal = GetCurrentWeekStartEastern();

            // 1) If this week is already stamped, return it.
            using (var checkExisting = new MySqlCommand(
                @"SELECT id, question_text
                  FROM Prompts
                  WHERE date_posted = @weekStart
                  LIMIT 1;", con))
            {
                checkExisting.Parameters.AddWithValue("@weekStart", weekStartLocal);
                using (var rdr = checkExisting.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new PromptInfo
                        {
                            id = rdr.GetInt32("id"),
                            text = rdr.GetString("question_text"),
                            weekStartIso = weekStartLocal.ToString("yyyy-MM-dd")
                        };
                    }
                }
            }

            // 2) Not stamped yet: acquire a named lock to avoid races.
            string lockName = "weekly_prompt_" + weekStartLocal.ToString("yyyyMMdd");
            using (var getLock = new MySqlCommand("SELECT GET_LOCK(@name, 5);", con))
            {
                getLock.Parameters.AddWithValue("@name", lockName);
                var lockResult = getLock.ExecuteScalar();
                bool haveLock = lockResult != null && Convert.ToInt32(lockResult) == 1;

                if (haveLock)
                {
                    try
                    {
                        // Re-check inside the lock (TOCTOU guard).
                        using (var recheck = new MySqlCommand(
                            @"SELECT id, question_text
                              FROM Prompts
                              WHERE date_posted = @weekStart
                              LIMIT 1;", con))
                        {
                            recheck.Parameters.AddWithValue("@weekStart", weekStartLocal);
                            using (var rdr = recheck.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    return new PromptInfo
                                    {
                                        id = rdr.GetInt32("id"),
                                        text = rdr.GetString("question_text"),
                                        weekStartIso = weekStartLocal.ToString("yyyy-MM-dd")
                                    };
                                }
                            }
                        }

                        // Build candidate set: all prompts not yet used.
                        // (We don't filter by insert date because stamping fixes the week's choice.)
                        int chosenId = -1;
                        string chosenText = null;

                        using (var pick = new MySqlCommand(
                            @"SELECT id, question_text
                              FROM Prompts
                              WHERE is_used = 0
                              ORDER BY RAND()
                              LIMIT 1;", con))
                        using (var rdr = pick.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                chosenId = rdr.GetInt32("id");
                                chosenText = rdr.GetString("question_text");
                            }
                        }

                        // If none remain, reset cycle and pick again.
                        if (chosenId == -1)
                        {
                            using (var reset = new MySqlCommand(
                                @"UPDATE Prompts SET is_used = 0, date_posted = NULL;", con))
                            {
                                reset.ExecuteNonQuery();
                            }

                            using (var pick2 = new MySqlCommand(
                                @"SELECT id, question_text
                                  FROM Prompts
                                  ORDER BY RAND()
                                  LIMIT 1;", con))
                            using (var rdr = pick2.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    chosenId = rdr.GetInt32("id");
                                    chosenText = rdr.GetString("question_text");
                                }
                            }
                        }

                        if (chosenId == -1)
                            return null; // no prompts in DB

                        // Stamp the choice for this week and mark used.
                        using (var stamp = new MySqlCommand(
                            @"UPDATE Prompts
                              SET date_posted = @weekStart, is_used = 1
                              WHERE id = @id;", con))
                        {
                            stamp.Parameters.AddWithValue("@weekStart", weekStartLocal);
                            stamp.Parameters.AddWithValue("@id", chosenId);
                            stamp.ExecuteNonQuery();
                        }

                        return new PromptInfo
                        {
                            id = chosenId,
                            text = chosenText ?? string.Empty,
                            weekStartIso = weekStartLocal.ToString("yyyy-MM-dd")
                        };
                    }
                    finally
                    {
                        using (var rel = new MySqlCommand("SELECT RELEASE_LOCK(@name);", con))
                        {
                            rel.Parameters.AddWithValue("@name", lockName);
                            rel.ExecuteScalar();
                        }
                    }
                }
                else
                {
                    // Couldn't acquire lock; another request is doing the stamping.
                    // Small wait and read the stamped row.
                    System.Threading.Thread.Sleep(200);

                    using (var waitRead = new MySqlCommand(
                        @"SELECT id, question_text
                          FROM Prompts
                          WHERE date_posted = @weekStart
                          LIMIT 1;", con))
                    {
                        waitRead.Parameters.AddWithValue("@weekStart", weekStartLocal);
                        using (var rdr = waitRead.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                return new PromptInfo
                                {
                                    id = rdr.GetInt32("id"),
                                    text = rdr.GetString("question_text"),
                                    weekStartIso = weekStartLocal.ToString("yyyy-MM-dd")
                                };
                            }
                        }
                    }

                    // As a final fallback, just return any prompt (should be rare).
                    using (var any = new MySqlCommand(
                        @"SELECT id, question_text
                          FROM Prompts
                          ORDER BY id ASC
                          LIMIT 1;", con))
                    using (var rdr = any.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new PromptInfo
                            {
                                id = rdr.GetInt32("id"),
                                text = rdr.GetString("question_text"),
                                weekStartIso = weekStartLocal.ToString("yyyy-MM-dd")
                            };
                        }
                    }
                    return null;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ENDPOINTS
        // ─────────────────────────────────────────────────────────────────────

        // Connectivity test
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string TestConnection()
        {
            try
            {
                const string testQuery = "SELECT 1";
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    using (var cmd = new MySqlCommand(testQuery, con))
                    {
                        con.Open();
                        cmd.ExecuteScalar();
                    }
                }
                return "Success!";
            }
            catch (Exception e)
            {
                return "Connection failed: " + e.Message;
            }
        }

        // Returns: "admin", "employee", or "invalid"
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Login(string username, string password)
        {
            bool isAdmin;
            int userId;

            if (DatabaseHelper.TryAuthenticate(getConString(), username, password, out isAdmin, out userId))
            {
                Session["userID"] = userId;
                Session["isAdmin"] = isAdmin;
                Session.Timeout = 30;  // minutes
                return isAdmin ? "admin" : "employee";
            }

            return "invalid";
        }

        // Weekly "current prompt" (stateful stamping; random selection; wraps after full cycle)
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public PromptInfo GetCurrentPrompt()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();
                    return ResolveOrStampWeeklyPrompt(con);
                }
            }
            catch
            {
                return null;
            }
        }

        // Binds feedback to THIS WEEK'S stamped prompt; prevents duplicate submits per user/prompt.
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SubmitFeedback(string message, bool isAnonymous)
        {
            if (Session["userID"] == null)
                return "not_authenticated";

            int userId = (int)Session["userID"];

            try
            {
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();

                    // Resolve/stamp this week's prompt (random selection inside if needed)
                    var prompt = ResolveOrStampWeeklyPrompt(con);
                    if (prompt == null)
                        return "no_current_prompt";

                    int promptId = prompt.id;

                    // Prevent duplicate submission for this user and this week's prompt.
                    using (var check = new MySqlCommand(
                        @"SELECT COUNT(*)
                          FROM Feedback
                          WHERE user_id = @uid AND prompt_id = @pid;", con))
                    {
                        check.Parameters.AddWithValue("@uid", userId);
                        check.Parameters.AddWithValue("@pid", promptId);

                        int count = Convert.ToInt32(check.ExecuteScalar());
                        if (count > 0)
                            return "already_submitted";
                    }

                    // Insert feedback
                    using (var insert = new MySqlCommand(
                        @"INSERT INTO Feedback (prompt_id, user_id, message, is_anonymous)
                          VALUES (@pid, @uid, @msg, @anon);", con))
                    {
                        insert.Parameters.AddWithValue("@pid", promptId);
                        insert.Parameters.AddWithValue("@uid", userId);
                        insert.Parameters.AddWithValue("@msg", message ?? string.Empty);
                        insert.Parameters.AddWithValue("@anon", isAnonymous);
                        insert.ExecuteNonQuery();
                    }

                    return "success";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        // ADMIN-ONLY: Read ALL feedback (paged) with prompt context.
        // Returns { totalCount, items: [ { id, dateSubmitted, displayName, isAnonymous, message, promptId, promptText, weekStartIso }, ... ] }
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public object GetAllFeedback(int page, int pageSize)
        {
            // Require admin
            if (!(Session["isAdmin"] is bool isAdmin && isAdmin))
                return new { error = "unauthorized" };

            // Normalize paging
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 100;
            if (pageSize > 500) pageSize = 500;
            int offset = (page - 1) * pageSize;

            try
            {
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();

                    // Total count (all feedback)
                    int total = 0;
                    using (var cnt = new MySqlCommand(
                        @"SELECT COUNT(*) FROM Feedback;", con))
                    {
                        total = Convert.ToInt32(cnt.ExecuteScalar());
                    }

                    var items = new List<FeedbackItem>();
                    using (var cmd = new MySqlCommand(
                        @"SELECT f.id,
                                 f.date_submitted,
                                 f.is_anonymous,
                                 u.username,
                                 f.message,
                                 f.prompt_id,
                                 p.question_text,
                                 p.date_posted AS week_start
                          FROM Feedback f
                          LEFT JOIN Users u ON u.id = f.user_id
                          LEFT JOIN Prompts p ON p.id = f.prompt_id
                          ORDER BY f.date_submitted DESC
                          LIMIT @limit OFFSET @offset;", con))
                    {
                        cmd.Parameters.AddWithValue("@limit", pageSize);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                bool isAnon = rdr.GetBoolean("is_anonymous");
                                DateTime dt = rdr.GetDateTime("date_submitted");
                                DateTime? wk = rdr.IsDBNull(rdr.GetOrdinal("week_start"))
                                    ? (DateTime?)null : rdr.GetDateTime("week_start");

                                string nameFromDb = rdr.IsDBNull(rdr.GetOrdinal("username")) ? null : rdr.GetString("username");
                                string displayName = isAnon ? "Anonymous" :
                                    (string.IsNullOrWhiteSpace(nameFromDb) ? "(unknown user)" : nameFromDb);

                                items.Add(new FeedbackItem
                                {
                                    id = rdr.GetInt32("id"),
                                    dateSubmitted = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    isAnonymous = isAnon,
                                    displayName = displayName,
                                    message = rdr.IsDBNull(rdr.GetOrdinal("message")) ? "" : rdr.GetString("message"),
                                    promptId = rdr.GetInt32("prompt_id"),
                                    promptText = rdr.IsDBNull(rdr.GetOrdinal("question_text")) ? "" : rdr.GetString("question_text"),
                                    weekStartIso = wk.HasValue ? wk.Value.ToString("yyyy-MM-dd") : ""
                                });
                            }
                        }
                    }

                    return new FeedbackListResult
                    {
                        totalCount = total,
                        items = items
                    };
                }
            }
            catch (Exception ex)
            {
                return new { error = "error", detail = ex.Message };
            }
        }
    }
}