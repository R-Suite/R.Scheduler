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

                        command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error persisting AuditLog. {0}", ex.Message);
                }
            }
        }

        public int GetJobDetailsCount()
        {
            int retval = 0;
            const string sql = @"SELECT count(*) FROM [QRTZ_JOB_DETAILS];";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        retval = (int) command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting job details count. {0}", ex.Message);
                }
            }

            return retval;
        }

        public int GetTriggerCount()
        {
            int retval = 0;
            const string sql = @"SELECT count(*) FROM [QRTZ_TRIGGERS];";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        retval = (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting triggers count. {0}", ex.Message);
                }
            }

            return retval;
        }

        public IList<TriggerKey> GetFiredTriggers()
        {
            IList<TriggerKey> keys = new List<TriggerKey>();

            const string sql = @"SELECT [TRIGGER_NAME], [TRIGGER_GROUP] FROM [QRTZ_FIRED_TRIGGERS]";

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
                        using (SqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                keys.Add(new TriggerKey(rs.GetString(0), rs.GetString(1)));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting fired triggers. {0}", ex.Message);
                }
            }

            return keys;
        }

        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT TOP {0} * FROM [RSCHED_AUDIT_HISTORY] WHERE [EXECUTION_EXCEPTION] <> '' AND [ACTION] = 'JobWasExecuted' order by [TIME_STAMP] DESC", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT TOP {0} * FROM [RSCHED_AUDIT_HISTORY] WHERE [ACTION] = 'JobWasExecuted' order by [TIME_STAMP] DESC", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        /// <summary>
        /// Insert JobKey and return new id.
        /// Return existing id if job key laready exists. 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup)
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
                        retval = Guid.NewGuid();
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
                    Logger.ErrorFormat("Error persisting Id-JobKey Map. {0}", ex.Message);
                    throw;
                }
            }

            return retval;
        }

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
                    Logger.ErrorFormat("Error getting JobKeys. {0}", ex.Message);
                }
            }

            return key;
        }

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
                    Logger.ErrorFormat("Error getting Job Id. {0}", ex.Message);
                }
            }

            return id;
        }

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
                    Logger.ErrorFormat("Error getting TriggerKeys. {0}", ex.Message);
                }
            }

            return key;
        }

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
                    Logger.ErrorFormat("Error persisting Id-TriggerKey Map. {0}", ex.Message);
                    throw;
                }
            }

            return retval;
        }

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
                    Logger.ErrorFormat("Error persisting Id-CalendarName Map. {0}", ex.Message);
                    throw;
                }
            }

            return retval;
        }

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
                    Logger.ErrorFormat("Error getting calendar name. {0}", ex.Message);
                }
            }

            return name;
        }

        private IEnumerable<AuditLog> GetAuditLogs(string sql)
        {
            IList<AuditLog> retval = new List<AuditLog>();

            using (var con = new SqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new SqlCommand(sql, con))
                    {
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
                                    Result = rs["RESULT"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting AuditLogs. {0}", ex.Message);
                }
            }
            return retval;
        }
    }
}
