using System;

namespace R.Scheduler.Contracts.Model
{
    public class CustomJobSimpleTrigger
    {
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }
        public string JobGroup { get; set; }

        public DateTime StartDateTime { get; set; }
        public int RepeatCount { get; set; }
        public TimeSpan RepeatInterval { get; set; }
    }
}
