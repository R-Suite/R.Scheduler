using System;
using System.Collections.Generic;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Interfaces
{
    public interface IAnalytics
    {
        int GetJobCount();
        int GetTriggerCount();
        IEnumerable<FireInstance> GetExecutingJobs(IEnumerable<string> authorizedJobGroups);
        IEnumerable<AuditLog> GetErroredJobs(int count, IEnumerable<string> authorizedJobGroups);
        IEnumerable<AuditLog> GetExecutedJobs(int count, IEnumerable<string> authorizedJobGroups);
        IEnumerable<FireInstance> GetUpcomingJobs(int count, IEnumerable<string> authorizedJobGroups);
        IEnumerable<FireInstance> GetUpcomingJobsBetween(DateTime from, DateTime to, IEnumerable<string> authorizedJobGroups);
        IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to, IEnumerable<string> authorizedJobGroups);
    }
}
