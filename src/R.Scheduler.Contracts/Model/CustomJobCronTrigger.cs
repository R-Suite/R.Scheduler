using System;

namespace R.Scheduler.Contracts.Model
{
    public class CustomJobCronTrigger
    {
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }
        public string JobGroup { get; set; }

        public DateTime StartDateTime { get; set; }
        public string CalendarName { get; set; }
        public string CronExpression { get; set; }
    }
}
