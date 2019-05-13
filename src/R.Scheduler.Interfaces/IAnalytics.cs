using System;
using System.Collections.Generic;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Interfaces
{
    public interface IAnalytics
    {
        int GetJobCount();
        int GetTriggerCount();
        IEnumerable<FireInstance> GetExecutingJobs();
        IEnumerable<AuditLog> GetErroredJobs(int count);
        IEnumerable<AuditLog> GetExecutedJobs(int count);
        IEnumerable<FireInstance> GetUpcomingJobs(int count);
        IEnumerable<FireInstance> GetUpcomingJobsBetween(DateTime from, DateTime to);
        IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to);
    }
}
