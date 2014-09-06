using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class GetRegisteredPluginsRequest : Message
    {
        public GetRegisteredPluginsRequest(Guid correlationId) : base(correlationId)
        {
        }
    }
}
