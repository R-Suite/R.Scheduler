using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface IPersistanceStore
    {
        void InsertAuditLog(AuditLog log);
        int GetJobDetailsCount();
        int GetTriggerCount();
        IList<TriggerKey> GetFiredTriggers();
        IEnumerable<AuditLog> GetErroredJobs(int count);
        IEnumerable<AuditLog> GetExecutedJobs(int count);
        Guid UpsertJobKeyIdMap(string jobName, string jobGroup);
        JobKey GetJobKey(Guid id);
        Guid GetJobId(JobKey jobKey);
        TriggerKey GetTriggerKey(Guid id);
        Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup);
        Guid UpsertCalendarIdMap(string name);
        string GetCalendarName(Guid id);
    }
}
