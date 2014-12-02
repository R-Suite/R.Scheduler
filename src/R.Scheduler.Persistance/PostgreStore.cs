using System;
using System.Reflection;
using Common.Logging;
using Npgsql;
using NpgsqlTypes;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// PostgreSQL implementation of <see cref="IPersistanceStore"/>
    /// </summary>
    public class PostgreStore : IPersistanceStore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
            const string sqlInsert = @"INSERT INTO RSCHED_AUDIT_HISTORY([time_stamp]
                                                               ,[action]
                                                               ,[fire_instance_id]
                                                               ,[job_name]
                                                               ,[job_group]
                                                               ,[job_type]
                                                               ,[trigger_name]
                                                               ,[trigger_group]
                                                               ,[fire_time_utc]
                                                               ,[scheduled_fire_time_utc]
                                                               ,[job_run_time]
                                                               ,[params]
                                                               ,[refire_count]
                                                               ,[recovering]
                                                               ,[result]
                                                               ,[execution_exception]) 
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
                        command.Parameters.Add(new NpgsqlParameter("fireTimeUtc", NpgsqlDbType.Date));
                        command.Parameters.Add(new NpgsqlParameter("scheduledFireTimeUtc", NpgsqlDbType.Date));
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
                    Logger.ErrorFormat("Error persisting AuditLog. {0}", ex.Message);
                }
            }
        }
    }
}
