using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class DescheduleTriggerGroup : Message
    {
        public DescheduleTriggerGroup(Guid correlationId)
            : base(correlationId)
        {}

        public string TriggerGroup { get; set; }
    }
}
