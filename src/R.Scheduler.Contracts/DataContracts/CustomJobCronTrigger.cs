using System;

namespace R.Scheduler.Contracts.DataContracts
{
    public class CustomJobCronTrigger
    {
        public string Name { get; set; }

        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }

        public DateTime StartDateTime { get; set; }
        public string CronExpression { get; set; }
    }
}
