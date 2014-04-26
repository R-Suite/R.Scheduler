using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public enum RepeatIntervalType
    {
        Seconds,
        Minutes,
        Hours,
        Days,
        Weeks,
        Months
    }

    public class SchedulePluginWithSimpleTrigger : Message
    {
        public SchedulePluginWithSimpleTrigger(Guid correlationId)
            : base(correlationId)
        {
        }

        public string PluginName { get; set; }
        public DateTime RunDateTime { get; set; }
        public int NumberOfRepeats { get; set; }
        public int RepeatInterval { get; set; }
        public RepeatIntervalType RepeatIntervalType { get; set; }

        public string JobName { get; set; }
        public string TriggerName { get; set; }
        public string GroupName { get; set; }
    }
}
