using System;
using System.Collections.Generic;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;

namespace R.Scheduler.AssemblyPlugin.Contracts.Messages
{
    public class GetPluginsRequest : Message
    {
        public GetPluginsRequest(Guid correlationId) : base(correlationId)
        {
        }
    }

    public class GetPluginsResponse : Message
    {
        public GetPluginsResponse(Guid correlationId)
            : base(correlationId)
        {
        }

        public List<Plugin> RegisteredPlugins { get; set; }
    }
}
