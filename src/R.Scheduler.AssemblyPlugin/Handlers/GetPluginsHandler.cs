using System.Collections.Generic;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.AssemblyPlugin.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class GetPluginsHandler : IMessageHandler<GetPluginsRequest>
    {
        readonly IPluginStore _pluginRepository;

        public GetPluginsHandler(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(GetPluginsRequest message)
        {
            var registeredPlugins = _pluginRepository.GetRegisteredPlugins();

            Context.Reply(new GetPluginsResponse(message.CorrelationId) { RegisteredPlugins = (List<Plugin>)registeredPlugins });
        }

        public IConsumeContext Context { get; set; }
    }
}
