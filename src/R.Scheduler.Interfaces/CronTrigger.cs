using System;
using System.Collections.Generic;

namespace R.Scheduler.Interfaces
{
    public class CronTrigger
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string JobGroup { get; set; }
        public string JobName { get; set; }

        public DateTime StartDateTime { get; set; }
        public string CronExpression { get; set; }

        public Dictionary<string, object> DataMap { get; set; } 
    }
}
