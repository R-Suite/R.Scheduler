using System;
using System.Collections.Generic;

namespace R.Scheduler.Interfaces
{
    public class SimpleTrigger
    {
        public string GroupName { get; set; }
        public string JobName { get; set; }
        public string TriggerName { get; set; }

        public DateTime StartDateTime { get; set; }
        public int RepeatCount { get; set; }
        public TimeSpan RepeatInterval { get; set; }

        public Dictionary<string, object> DataMap { get; set; } 
    }
}
