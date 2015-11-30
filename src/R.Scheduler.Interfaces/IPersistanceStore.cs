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
    }
}
