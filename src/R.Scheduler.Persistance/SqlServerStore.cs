using System;
using System.Data.SqlClient;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class SqlServerStore : IPersistanceStore
    {
        private readonly string _connectionString;

        public SqlServerStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertAuditLog(AuditLog log)
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sqlInsert = @"INSERT INTO RSCHED_AUDIT_HISTORY([TIME_STAMP]
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
            var command = new SqlCommand(sqlInsert, conn);

            try
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
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
                        command.Parameters.AddWithValue("@result", log.Result);
                        command.Parameters.AddWithValue("@executionException", log.ExecutionException);

                        command.Transaction = transaction;
                        command.CommandText = sqlInsert;
                        command.ExecuteScalar();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
