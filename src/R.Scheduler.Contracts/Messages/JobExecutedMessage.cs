using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class JobExecutedMessage : Message
    {
        public JobExecutedMessage(Guid id)
            : base(id)
        {
        }

        public bool Success { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
