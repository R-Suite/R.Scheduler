using System;

namespace R.Scheduler.Interfaces
{
    public interface IAuditLog
    {
        Guid Id { get; set; }
        DateTime FireTimeUtc { get; set; }
        DateTime ExitTimeUtc { get; set; }
        string JobName { get; set; }
        string JobGroup { get; set; }
        string JobType { get; set; }
        string TriggerName { get; set; }
        string TriggerGroup { get; set; }
        string ErrorMessage { get; set; }
        string ExecutionStatus { get; set; }
        int RefireCount { get; set; }
    }
}