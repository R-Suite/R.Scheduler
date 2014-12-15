using System;

namespace R.Scheduler.Contracts.Model
{
    public class TriggerFireTime
    {
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public DateTimeOffset FireDateTime { get; set; }
    }
}
