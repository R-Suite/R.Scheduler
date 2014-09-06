using System.Collections.Generic;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class GetRegisteredPluginsHandler : IMessageHandler<GetRegisteredPluginsRequest>
    {
        readonly IPluginStore _pluginRepository;

        public GetRegisteredPluginsHandler(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(GetRegisteredPluginsRequest message)
        {
            var registeredPlugins = _pluginRepository.GetRegisteredPlugins();

            Context.Reply(new GetRegisteredPluginsResponse(message.CorrelationId) { RegisteredPlugins = (List<Plugin>) registeredPlugins});
        }

        public IConsumeContext Context { get; set; }
    }
}
