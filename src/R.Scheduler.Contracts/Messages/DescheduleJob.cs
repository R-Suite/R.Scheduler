using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DescheduleJob : Message
    {
        public DescheduleJob(Guid correlationId)
            : base(correlationId)
        {}

        public string JobName { get; set; }
    }
}
