using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface IPersistanceStore
    {
        void InsertAuditLog(AuditLog log);
        IEnumerable<AuditLog> GetErroredJobs(int count);
        IEnumerable<AuditLog> GetExecutedJobs(int count);
        Guid UpsertJobKeyIdMap(string jobName, string jobGroup, Guid? jobId = null);
        void RemoveJobKeyIdMap(string jobName, string jobGroup);
        JobKey GetJobKey(Guid id);
        Guid GetJobId(JobKey jobKey);
        TriggerKey GetTriggerKey(Guid id);
        Guid GetTriggerId(TriggerKey triggerKey);
        Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup);
        void RemoveTriggerKeyIdMap(string triggerName, string triggerGroup);
        Guid UpsertCalendarIdMap(string name);
        string GetCalendarName(Guid id);
        void RemoveCalendarIdMap(string name);
        Guid GetCalendarId(string name);
        IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to);
    }
}
