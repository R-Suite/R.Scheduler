using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DescheduleJob : Message
    {
        public DescheduleJob(Guid correlationId) : base(correlationId)
        {}

        public string PluginName { get; set; }

        public string JobName { get; set; }
        public string TriggerName { get; set; }
        public string GroupName { get; set; }
    }
}
