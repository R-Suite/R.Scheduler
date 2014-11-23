namespace R.Scheduler.Contracts.JobTypes
{
    public class BaseJob
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string SchedulerName { get; set; }
    }
}
