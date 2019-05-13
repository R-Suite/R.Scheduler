using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Common.Logging;
using Quartz;
using Quartz.Util;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// Sql Server implementation of <see cref="IPersistanceStore"/>
    /// </summary>
    public class SqlServerStore : IPersistanceStore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog AuditLogLogger = LogManager.GetLogger("AuditLog");

        private readonly string _connectionString;

        public SqlServerStore(string connectionString)
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
            const string sqlInsert = @"INSERT INTO RSCHED_AUDIT_HISTORY([TIME_STAMP]
                                                               ,[ACTION]
                                                               ,[FIRE_INSTANCE_ID]
                                                               ,[JOB_NAME]
                                                               ,[JOB_GROUP]
                                                               ,[JOB_TYPE]
                                                               ,[TRIGGER_NAME]
                                                               ,[TRIGGER_GROUP]
                                                               ,[FIRE_TIME_UTC]
                                                               ,[SCHEDULED_FIRE_TIME_UTC]
                                                               ,[JOB_RUN_TIME]
                                                               ,[PARAMS]
                                                               ,[REFIRE_COUNT]
                                                               ,[RECOVERING]
                                                               ,[RESULT]
                                                               ,[EXECUTION_EXCEPTION]) 
                                                            VALUES (
                                                                @timeStamp, 
                                                                @action, 
                                                                @fireInstanceId, 
                                                                @jobName, 
                                                                @jobGroup, 
                                                                @jobType, 
                                                                @triggerName, 
                                                                @triggerGroup, 
                                                                @fireTimeUtc, 
                                                                @scheduledFireTimeUtc, 
                                                                @jobRunTime, 
                                                                @params, 
                                                                @refireCount, 
                                                                @recovering, 
                                                                @result, 
                                                                @executionException);";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sqlInsert, con))
                    {
                        command.Parameters.AddWithValue("@timeStamp", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@action", log.Action);
                        command.Parameters.AddWithValue("@fireInstanceId", log.FireInstanceId);
                        command.Parameters.AddWithValue("@jobName", log.JobName);
                        command.Parameters.AddWithValue("@jobGroup", log.JobGroup);
                        command.Parameters.AddWithValue("@jobType", log.JobType);
                        command.Parameters.AddWithValue("@triggerName", log.TriggerName);
                        command.Parameters.AddWithValue("@triggerGroup", log.TriggerGroup);
                        command.Parameters.AddWithValue("@fireTimeUtc", log.FireTimeUtc);
                        command.Parameters.AddWithValue("@scheduledFireTimeUtc", log.ScheduledFireTimeUtc);
                        command.Parameters.AddWithValue("@jobRunTime", log.JobRunTime.Ticks);
                        command.Parameters.AddWithValue("@params", log.Params);
                        command.Parameters.AddWithValue("@refireCount", log.RefireCount);
                        command.Parameters.AddWithValue("@recovering", log.Recovering);
                        command.Parameters.AddWithValue("@result", log.Result ?? string.Empty);
                        command.Parameters.AddWithValue("@executionException", log.ExecutionException ?? string.Empty);

                        command.CommandTimeout = 60;
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

            string sql = string.Format(@"SELECT TOP {0} a.*, m.[ID] as 'JOB_ID' FROM [RSCHED_AUDIT_HISTORY] a, [RSCHED_JOB_ID_KEY_MAP] m WHERE a.[EXECUTION_EXCEPTION] <> '' AND a.[ACTION] = 'JobWasExecuted' AND a.[JOB_NAME] = m.[JOB_NAME] AND a.[JOB_GROUP] = m.[JOB_GROUP] order by a.[TIME_STAMP] DESC", count);

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

            string sql = string.Format(@"SELECT TOP {0} a.*, m.[ID] as 'JOB_ID'  FROM [RSCHED_AUDIT_HISTORY] a, [RSCHED_JOB_ID_KEY_MAP] m WHERE a.[ACTION] = 'JobWasExecuted' AND a.[JOB_NAME] = m.[JOB_NAME] AND a.[JOB_GROUP] = m.[JOB_GROUP] order by a.[TIME_STAMP] DESC", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        /// <summary>
        /// Insert JobKey and return new (or provided) id.
        /// Return existing id if job key already exists. 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup, Guid? jobId = null)
        {
            var retval = Guid.Empty;

            const string sql = @"SELECT [ID] FROM [RSCHED_JOB_ID_KEY_MAP] WHERE [JOB_NAME] = @jobName AND [JOB_GROUP] = @jobGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new SqlCommand(sql, con, trans))
                    {
                        command.Parameters.AddWithValue("@jobName", jobName);
                        command.Parameters.AddWithValue("@jobGroup", jobGroup);

                        using (IDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                retval = dr.GetGuid(0);
                            }
                        }
                    }

                    // JobKey does not exist
                    if (retval == Guid.Empty)
                    {
                        retval = jobId == null ? Guid.NewGuid() : jobId.Value;

                        const string sqlInsert = @"INSERT INTO RSCHED_JOB_ID_KEY_MAP([ID]
                                                               ,[JOB_NAME]
                                                               ,[JOB_GROUP]) 
                                                            VALUES (
                                                                @id, 
                                                                @jobName, 
                                                                @jobGroup);";
                        using (var command = new SqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.AddWithValue("@id", retval);
                            command.Parameters.AddWithValue("@jobName", jobName);
                            command.Parameters.AddWithValue("@jobGroup", jobGroup);

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
        /// Delete jobKey-id map.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void RemoveJobKeyIdMap(string jobName, string jobGroup)
        {
            const string sql = @"DELETE FROM [RSCHED_JOB_ID_KEY_MAP] WHERE [JOB_NAME] = @jobName AND [JOB_GROUP] = @jobGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("@jobName", jobName);
                        command.Parameters.AddWithValue("@jobGroup", jobGroup);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deleting Id-JobKey Map.", ex);
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

            const string sql = @"SELECT [JOB_NAME], [JOB_GROUP] FROM [RSCHED_JOB_ID_KEY_MAP] WHERE [ID] = @id";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("id", id);
                        using (SqlDataReader rs = command.ExecuteReader())
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

            const string sql = @"SELECT [ID] FROM [RSCHED_JOB_ID_KEY_MAP] WHERE [JOB_NAME] = @jobName AND [JOB_GROUP] = @jobGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("jobName", jobKey.Name);
                        command.Parameters.AddWithValue("jobGroup", jobKey.Group);
                        using (SqlDataReader rs = command.ExecuteReader())
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

            const string sql = @"SELECT [TRIGGER_NAME], [TRIGGER_GROUP] FROM [RSCHED_TRIGGER_ID_KEY_MAP] WHERE [ID] = @id";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("id", id);
                        using (SqlDataReader rs = command.ExecuteReader())
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
                    Logger.Error("Error getting TriggerKeys. {0}", ex);
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

            const string sql = @"SELECT [ID] FROM [RSCHED_TRIGGER_ID_KEY_MAP] WHERE [TRIGGER_NAME] = @triggerName AND [TRIGGER_GROUP] = @triggerGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("triggerName", triggerKey.Name);
                        command.Parameters.AddWithValue("triggerGroup", triggerKey.Group);
                        using (SqlDataReader rs = command.ExecuteReader())
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
        /// Delete triggerKey - id map.
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        public void RemoveTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            const string sql = @"DELETE FROM [RSCHED_TRIGGER_ID_KEY_MAP] WHERE [TRIGGER_NAME] = @triggerName AND [TRIGGER_GROUP] = @triggerGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("@triggerName", triggerName);
                        command.Parameters.AddWithValue("@triggerGroup", triggerGroup);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deleting Id-TriggerKey Map. {0}", ex);
                }
            }
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

            const string sql = @"SELECT [ID] FROM [RSCHED_TRIGGER_ID_KEY_MAP] WHERE [TRIGGER_NAME] = @triggerName AND [TRIGGER_GROUP] = @triggerGroup";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new SqlCommand(sql, con, trans))
                    {
                        command.Parameters.AddWithValue("@triggerName", triggerName);
                        command.Parameters.AddWithValue("@triggerGroup", triggerGroup);

                        using (IDataReader dr = command.ExecuteReader())
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
                        const string sqlInsert = @"INSERT INTO RSCHED_TRIGGER_ID_KEY_MAP([ID]
                                                               ,[TRIGGER_NAME]
                                                               ,[TRIGGER_GROUP]) 
                                                            VALUES (
                                                                @id, 
                                                                @triggerName, 
                                                                @triggerGroup);";
                        using (var command = new SqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.AddWithValue("@id", retval);
                            command.Parameters.AddWithValue("@triggerName", triggerName);
                            command.Parameters.AddWithValue("@triggerGroup", triggerGroup);

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
        /// Insert calendar name and return a new id.
        /// If calendar name already exists, do nothing and return existing id.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid UpsertCalendarIdMap(string name)
        {
            var retval = Guid.Empty;

            const string sql = @"SELECT [ID] FROM [RSCHED_CALENDAR_ID_NAME_MAP] WHERE [CALENDAR_NAME] = @calendarName";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (var command = new SqlCommand(sql, con, trans))
                    {
                        command.Parameters.AddWithValue("@calendarName", name);

                        using (IDataReader dr = command.ExecuteReader())
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
                        const string sqlInsert = @"INSERT INTO RSCHED_CALENDAR_ID_NAME_MAP([ID]
                                                               ,[CALENDAR_NAME]) 
                                                            VALUES (
                                                                @id, 
                                                                @calendarName);";
                        using (var command = new SqlCommand(sqlInsert, con, trans))
                        {
                            command.Parameters.AddWithValue("@id", retval);
                            command.Parameters.AddWithValue("@calendarName", name);

                            command.ExecuteScalar();
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.ErrorFormat("Error persisting Id-CalendarName Map.", ex);
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

            const string sql = @"SELECT [CALENDAR_NAME] FROM [RSCHED_CALENDAR_ID_NAME_MAP] WHERE [ID] = @id";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("id", id);
                        using (SqlDataReader rs = command.ExecuteReader())
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
        /// Delete Calendar id mapping
        /// </summary>
        /// <param name="name"></param>
        public void RemoveCalendarIdMap(string name)
        {
            const string sql = @"DELETE FROM [RSCHED_CALENDAR_ID_NAME_MAP] WHERE [CALENDAR_NAME] = @name";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                try
                {
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deleting Id-CalendarName Map.", ex);
                }
            }
        }

        /// <summary>
        /// Get calendar id mapped to specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid GetCalendarId(string name)
        {
            Guid id = Guid.Empty;

            const string sql = @"SELECT [ID] FROM [RSCHED_CALENDAR_ID_NAME_MAP] WHERE [CALENDAR_NAME] = @name";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("name", name);
                        using (SqlDataReader rs = command.ExecuteReader())
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
                    Logger.ErrorFormat("Error getting calendar id.", ex);
                }
            }

            return id;
        }

        /// <summary>
        /// Get <see cref="AuditLog"/> of executed jobs within a specified date range
        /// </summary>
        /// <param name="id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to)
        {
            string sql = @"SELECT TOP 1000 a.*, m.[ID] as 'JOB_ID'  FROM [RSCHED_AUDIT_HISTORY] a, [RSCHED_JOB_ID_KEY_MAP] m WHERE a.[ACTION] = 'JobWasExecuted' AND a.[JOB_NAME] = m.[JOB_NAME] AND a.[JOB_GROUP] = m.[JOB_GROUP] AND m.[ID] = @id AND a.[SCHEDULED_FIRE_TIME_UTC] > @from AND a.[SCHEDULED_FIRE_TIME_UTC] < @to order by a.[TIME_STAMP] DESC";

            IList<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", id));
            parameters.Add(new SqlParameter("from", from));
            parameters.Add(new SqlParameter("to", to));

            IEnumerable<AuditLog> retval = GetAuditLogs(sql, parameters);

            return retval;
        }

        private IEnumerable<AuditLog> GetAuditLogs(string sql, IEnumerable<SqlParameter> parameters = null)
        {
            IList<AuditLog> retval = new List<AuditLog>();

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                command.Parameters.Add(p);
                            }
                        }

                        using (IDataReader rs = command.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                retval.Add(new AuditLog
                                {
                                    TimeStamp = (DateTime) rs["TIME_STAMP"],
                                    Action = rs.GetString("ACTION"),
                                    ExecutionException = rs["EXECUTION_EXCEPTION"].ToString(),
                                    FireInstanceId = rs["FIRE_INSTANCE_ID"].ToString(),
                                    FireTimeUtc = (DateTimeOffset?) rs["FIRE_TIME_UTC"],
                                    JobGroup = rs.GetString("JOB_GROUP"),
                                    JobName = rs.GetString("JOB_NAME"),
                                    JobType = rs.GetString("JOB_TYPE"),
                                    TriggerName = rs.GetString("TRIGGER_NAME"),
                                    TriggerGroup = rs.GetString("TRIGGER_GROUP"),
                                    JobRunTime = new TimeSpan((long) rs["JOB_RUN_TIME"]),
                                    ScheduledFireTimeUtc = (DateTimeOffset?) rs["SCHEDULED_FIRE_TIME_UTC"],
                                    Params = rs["PARAMS"].ToString(),
                                    RefireCount = (int) rs["REFIRE_COUNT"],
                                    Recovering = (bool) rs["RECOVERING"],
                                    Result = rs["RESULT"].ToString(),
                                    JobId = (Guid)rs["JOB_ID"]
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
