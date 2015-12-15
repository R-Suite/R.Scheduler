using System;

namespace R.Scheduler.Contracts.JobTypes
{
    public class BaseJob
    {
        public Guid Id { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string JobType { get; set; }
        public string SchedulerName { get; set; }
        public string Description { get; set; }
    }
}
