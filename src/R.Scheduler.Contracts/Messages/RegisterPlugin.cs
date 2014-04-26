using System;
using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Messages
{
    public class RegisterPlugin : Message
    {
        public RegisterPlugin(Guid correlationId) : base(correlationId)
        {
        }

        public string PluginName { get; set; }
        public string AssemblyPath { get; set; }
    }
}
