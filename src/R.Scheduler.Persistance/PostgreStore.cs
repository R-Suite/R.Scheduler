using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// PostgreSQL implementation of <see cref="IPersistanceStore"/>
    /// </summary>
    public class PostgreStore : IPersistanceStore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog AuditLogLogger = LogManager.GetLogger("AuditLog");
        private readonly string _connectionString;

        public PostgreStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Insert AuditLog. 
        /// Each entry is read-only.
        /// </summary>
        /// <param name="log"></param>
        public void InsertAuditLog(AuditLog log)
        {
            const string sqlInsert = @"INSERT INTO RSCHED_AUDIT_HISTORY(time_stamp
                                                               ,action
                                                               ,fire_instance_id
                                                               ,job_name
                                                               ,job_group
                                                               ,job_type
                                                               ,trigger_name
                                                               ,trigger_group
                                                               ,fire_time_utc
                                                               ,scheduled_fire_time_utc
                                                               ,job_run_time
                                                               ,params
                                                               ,refire_count
                                                               ,recovering
                                                               ,result
                                                               ,execution_exception) 
                                                            VALUES (
                                                                :timeStamp, 
                                                                :action, 
                                                                :fireInstanceId, 
                                                                :jobName, 
                                                                :jobGroup, 
                                                                :jobType, 
                                                                :triggerName, 
                                                                :triggerGroup, 
                                                                :fireTimeUtc, 
                                                                :scheduledFireTimeUtc, 
                                                                :jobRunTime, 
                                                                :params, 
                                                                :refireCount, 
                                                                :recovering, 
                                                                :result, 
                                                                :executionException);";


            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sqlInsert, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("timeStamp", NpgsqlDbType.Timestamp));
                        command.Parameters.Add(new NpgsqlParameter("action", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("fireInstanceId", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobType", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("fireTimeUtc", NpgsqlDbType.TimestampTZ));
                        command.Parameters.Add(new NpgsqlParameter("scheduledFireTimeUtc", NpgsqlDbType.TimestampTZ));
                        command.Parameters.Add(new NpgsqlParameter("jobRunTime", NpgsqlDbType.Bigint));
                        command.Parameters.Add(new NpgsqlParameter("params", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("refireCount", NpgsqlDbType.Integer));
                        command.Parameters.Add(new NpgsqlParameter("recovering", NpgsqlDbType.Boolean));
                        command.Parameters.Add(new NpgsqlParameter("result", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("executionException", NpgsqlDbType.Varchar));

                        command.Parameters[0].Value = DateTime.UtcNow;
                        command.Parameters[1].Value = log.Action;
                        command.Parameters[2].Value = log.FireInstanceId;
                        command.Parameters[3].Value = log.JobName;
                        command.Parameters[4].Value = log.JobGroup;
                        command.Parameters[5].Value = log.JobType;
                        command.Parameters[6].Value = log.TriggerName;
                        command.Parameters[7].Value = log.TriggerGroup;
                        command.Parameters[8].Value = log.FireTimeUtc;
                        command.Parameters[9].Value = log.ScheduledFireTimeUtc;
                        command.Parameters[10].Value = log.JobRunTime.Ticks;
                        command.Parameters[11].Value = log.Params;
                        command.Parameters[12].Value = log.RefireCount;
                        command.Parameters[13].Value = log.Recovering;
                        command.Parameters[14].Value = log.Result ?? string.Empty;
                        command.Parameters[15].Value = log.ExecutionException ?? string.Empty;

                        command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    AuditLogLogger.Error("Error persisting AuditLog.", ex);
                }
            }
        }

        /// <summary>
        /// Get <see cref="count"/> of most recently failed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT a.*, m.id as ""job_id"" FROM rsched_audit_history a, rsched_job_id_key_map m WHERE a.execution_exception <> '' AND a.action = 'JobWasExecuted' AND a.job_name = m.job_name AND a.job_group = m.job_group order by a.time_stamp desc limit {0}", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        /// <summary>
        /// Get <see cref="count"/> of most recently executed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT a.*, m.id as ""job_id"" FROM rsched_audit_history a, rsched_job_id_key_map m WHERE a.action = 'JobWasExecuted' AND a.job_name = m.job_name AND a.job_group = m.job_group order by a.time_stamp desc limit {0}", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        /// <summary>
        /// Insert JobKey and return new (or provided) id.
        /// Return existing id if job key laready exists. 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup, Guid? jobId = null)
        {
            var retval = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_job_id_key_map WHERE job_name = :jobName AND job_group = :jobGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                NpgsqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con, trans))
                    {
                        command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = jobName;
                        command.Parameters[1].Value = jobGroup;

                        using (NpgsqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                retval = dr.GetGuid(0);
                            }
                        }
                    }

                    // TriggerKey does not exist
                    if (retval == Guid.Empty)
                    {
                        retval = jobId == null ? Guid.NewGuid() : jobId.Value;

                        const string sqlInsert = @"INSERT INTO rsched_job_id_key_map(id
                                                               ,job_name
                                                               ,job_group) 
                                                            VALUES (
                                                                :id, 
                                                                :jobName, 
                                                                :jobGroup);";
                        using (var command = new NpgsqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                            command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                            command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                            command.Parameters[0].Value = retval;
                            command.Parameters[1].Value = jobName;
                            command.Parameters[2].Value = jobGroup;


                            command.ExecuteScalar();
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.Error("Error persisting Id-JobKey Map.", ex);
                    throw;
                }
            }

            return retval;
        }

        /// <summary>
        /// Delete JobKey id mapping
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void RemoveJobKeyIdMap(string jobName, string jobGroup)
        {
            const string sql = @"DELETE FROM rsched_job_id_key_map WHERE job_name = :jobName AND job_group = :jobGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = jobName;
                        command.Parameters[1].Value = jobGroup;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error persisting Id-JobKey Map.", ex);
                }
            }
        }

        /// <summary>
        /// Get JobKey mapped to specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JobKey GetJobKey(Guid id)
        {
            JobKey key = null;

            const string sql = @"SELECT job_name, job_group FROM rsched_job_id_key_map WHERE id = :id";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters[0].Value = id;

                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                key = new JobKey(rs.GetString(0), rs.GetString(1));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting JobKeys.", ex);
                }
            }

            return key;
        }

        /// <summary>
        /// Get JobId mapped to specified job key
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Guid GetJobId(JobKey jobKey)
        {
            Guid id = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_job_id_key_map WHERE job_name = :jobName AND job_group = :jobGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = jobKey.Name;
                        command.Parameters[1].Value = jobKey.Group;
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                id = rs.GetGuid(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting Job Id.", ex);
                }
            }

            return id;
        }

        /// <summary>
        /// Get TriggerKey mapped to specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TriggerKey GetTriggerKey(Guid id)
        {
            TriggerKey key = null;

            const string sql = @"SELECT trigger_name, trigger_group FROM rsched_trigger_id_key_map WHERE id = :id";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters[0].Value = id;

                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                key = new TriggerKey(rs.GetString(0), rs.GetString(1));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting TriggerKeys.", ex);
                }
            }

            return key;
        }

        /// <summary>
        /// Get TriggerId mapped to specified trigger key
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <returns></returns>
        public Guid GetTriggerId(TriggerKey triggerKey)
        {
            Guid id = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_trigger_id_key_map WHERE trigger_name = :triggerName AND trigger_group = :triggerGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = triggerKey.Name;
                        command.Parameters[1].Value = triggerKey.Group;
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                id = rs.GetGuid(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting Trigger Id.", ex);
                }
            }

            return id;
        }

        /// <summary>
        /// Insert trigger key and return a new trigger id.
        /// If trigger key already exists, do nothing and return existing trigger id.
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        /// <returns></returns>
        public Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            var retval = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_trigger_id_key_map WHERE trigger_name = :triggerName AND trigger_group = :triggerGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                NpgsqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con, trans))
                    {
                        command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = triggerName;
                        command.Parameters[1].Value = triggerGroup;

                        using (NpgsqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                retval = dr.GetGuid(0);
                            }
                        }
                    }

                    // TriggerKey does not exist
                    if (retval == Guid.Empty)
                    {
                        retval = Guid.NewGuid();
                        const string sqlInsert = @"INSERT INTO rsched_trigger_id_key_map(id
                                                               ,trigger_name
                                                               ,trigger_group) 
                                                            VALUES (
                                                                :id, 
                                                                :triggerName, 
                                                                :triggerGroup);";
                        using (var command = new NpgsqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                            command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                            command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                            command.Parameters[0].Value = retval;
                            command.Parameters[1].Value = triggerName;
                            command.Parameters[2].Value = triggerGroup;


                            command.ExecuteScalar();
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.Error("Error persisting Id-TriggerKey Map.", ex);
                    throw;
                }
            }

            return retval;
        }

        /// <summary>
        /// Delete TriggerKey id mapping
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        public void RemoveTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            const string sql = @"DELETE FROM rsched_trigger_id_key_map WHERE trigger_name = :triggerName AND trigger_group = :triggerGroup";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = triggerName;
                        command.Parameters[1].Value = triggerGroup;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error persisting Id-TriggerKey Map.", ex);
                }
            }
        }

        /// <summary>
        /// Insert calendar name and return a new id.
        /// If calendar name already exists, do nothing and return existing id.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid UpsertCalendarIdMap(string name)
        {
            var retval = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_calendar_id_name_map WHERE calendar_name = :calendarName";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                NpgsqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con, trans))
                    {
                        command.Parameters.Add(new NpgsqlParameter("calendarName", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = name;

                        using (NpgsqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                retval = dr.GetGuid(0);
                            }
                        }
                    }

                    // CalendarName does not exist
                    if (retval == Guid.Empty)
                    {
                        retval = Guid.NewGuid();
                        const string sqlInsert = @"INSERT INTO rsched_calendar_id_name_map(id,calendar_name) 
                                                          VALUES (:id, :calendarName);";
                        using (var command = new NpgsqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                            command.Parameters.Add(new NpgsqlParameter("calendarName", NpgsqlDbType.Varchar));
                            command.Parameters[0].Value = retval;
                            command.Parameters[1].Value = name;

                            command.ExecuteScalar();
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.Error("Error persisting Id-CalendarName Map.", ex);
                    throw;
                }
            }

            return retval;
        }

        /// <summary>
        /// Get calendar name mapped to specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetCalendarName(Guid id)
        {
            string name = null;

            const string sql = @"SELECT calendar_name FROM rsched_calendar_id_name_map WHERE id = :id";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters[0].Value = id;
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                name = rs.GetString(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting calendar name.", ex);
                }
            }

            return name;
        }

        /// <summary>
        ///  Get calendar id mapped to specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid GetCalendarId(string name)
        {
            Guid guid = Guid.Empty;

            const string sql = @"SELECT id FROM rsched_calendar_id_name_map WHERE calendar_name = :name";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text));
                        command.Parameters[0].Value = name;
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                guid = rs.GetGuid(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting calendar id.", ex);
                }
            }

            return guid;
        }

        /// <summary>
        /// Delete Calendar id mapping
        /// </summary>
        /// <param name="name"></param>
        public void RemoveCalendarIdMap(string name)
        {
            const string sql = @"DELETE FROM rsched_calendar_id_name_map WHERE calendar_name = :name";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar));
                        command.Parameters[0].Value = name;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deleting Id-CalendarName Map.", ex);
                }
            }
        }

        private IEnumerable<AuditLog> GetAuditLogs(string sql)
        {
            IList<AuditLog> retval = new List<AuditLog>();

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                Guid jobId;
                                if (!Guid.TryParse(rs["job_id"].ToString(), out jobId))
                                {
                                    jobId = Guid.Empty;
                                }
                                retval.Add(new AuditLog
                                {
                                    TimeStamp = (DateTime) rs["time_stamp"],
                                    Action = rs["action"].ToString(),
                                    ExecutionException = rs["execution_exception"].ToString(),
                                    FireInstanceId = rs["fire_instance_id"].ToString(),
                                    FireTimeUtc = DateTime.SpecifyKind((DateTime)rs["fire_time_utc"], DateTimeKind.Utc),
                                    JobGroup = rs["job_group"].ToString(),
                                    JobName = rs["job_name"].ToString(),
                                    JobType = rs["job_type"].ToString(),
                                    TriggerName = rs["trigger_name"].ToString(),
                                    TriggerGroup = rs["trigger_group"].ToString(),
                                    JobRunTime = new TimeSpan((long) rs["job_run_time"]),
                                    ScheduledFireTimeUtc = DateTime.SpecifyKind((DateTime)rs["scheduled_fire_time_utc"], DateTimeKind.Utc),
                                    Params = rs["params"].ToString(),
                                    RefireCount = (int) rs["refire_count"],
                                    Recovering = (bool) rs["recovering"],
                                    Result = rs["result"].ToString(),
                                    JobId = jobId
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AuditLogLogger.Error("Error getting AuditLogs.", ex);
                }
            }
            return retval;
        }
    }
}
