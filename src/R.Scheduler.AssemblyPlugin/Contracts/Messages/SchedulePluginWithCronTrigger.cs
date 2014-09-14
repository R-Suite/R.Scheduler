using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Contracts.Messages
{
    public class SchedulePluginWithCronTrigger : Message
    {
        public SchedulePluginWithCronTrigger(Guid correlationId)
            : base(correlationId)
        {
        }

        public string PluginName { get; set; }

        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }

        public DateTime StartDateTime { get; set; }
        public string CronExpression { get; set; }
    }
}
