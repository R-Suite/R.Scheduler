using System;
using System.Collections.Generic;

namespace R.Scheduler.Contracts.Model
{
    public abstract class BaseTrigger
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string JobGroup { get; set; }
        public string JobName { get; set; }

        public string CalendarName { get; set; }

        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// If provided, the JobDataMap will be available when your job executes, or in trigger listeners
        /// </summary>
        public Dictionary<string, object> JobDataMap { get; set; }
    }
}
