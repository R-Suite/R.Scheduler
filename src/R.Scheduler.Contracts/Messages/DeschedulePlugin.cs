using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DeschedulePlugin : Message
    {
        public DeschedulePlugin(Guid correlationId) : base(correlationId)
        {
        }

        public string PluginName { get; set; }
    }
}
