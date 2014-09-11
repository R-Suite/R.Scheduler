using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DescheduleTrigger : Message
    {
        public DescheduleTrigger(Guid correlationId)
            : base(correlationId)
        {}

        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
    }
}
