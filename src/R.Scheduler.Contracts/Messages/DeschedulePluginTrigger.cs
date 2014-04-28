using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DeschedulePluginTrigger : Message
    {
        public DeschedulePluginTrigger(Guid correlationId) : base(correlationId)
        {}

        public string PluginName { get; set; }
        public string TriggerName { get; set; }
    }
}
