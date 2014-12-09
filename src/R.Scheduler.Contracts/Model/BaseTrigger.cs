using System;

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
    }
}
