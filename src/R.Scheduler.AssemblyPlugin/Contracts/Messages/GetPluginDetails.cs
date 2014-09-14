using System;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;

namespace R.Scheduler.AssemblyPlugin.Contracts.Messages
{
    public class GetPluginDetailsRequest : Message
    {
        public GetPluginDetailsRequest(Guid correlationId)
            : base(correlationId)
        {
        }
        public string PluginName { get; set; }
    }

    public class GetPluginDetailsResponse : Message
    {
        public GetPluginDetailsResponse(Guid correlationId)
            : base(correlationId)
        {
        }
        public PluginDetails PluginDetails { get; set; }
    }
}
