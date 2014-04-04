using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class ScheduleOneTimeJobCommand : Message
    {
        public ScheduleOneTimeJobCommand(Guid correlationId) : base(correlationId)
        {
        }

        public string JobName { get; set; }
        public string GroupName { get; set; }
        public string TriggerName { get; set; }
        public DateTime RunDateTime { get; set; }
        public string PluginName { get; set; }
    }
}
