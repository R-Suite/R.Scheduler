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
