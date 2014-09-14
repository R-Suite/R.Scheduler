using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Contracts.Messages
{
    public class ExecutePlugin : Message
    {
        public ExecutePlugin(Guid correlationId) : base(correlationId)
        {
        }

        public string PluginName { get; set; }
    }
}
