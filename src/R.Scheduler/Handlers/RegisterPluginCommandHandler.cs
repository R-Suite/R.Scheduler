using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class RegisterPluginCommandHandler : IMessageHandler<RegisterPluginCommand>
    {
        readonly IPluginStore _pluginRepository;

        public RegisterPluginCommandHandler(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(RegisterPluginCommand command)
        {
            //todo: verify valid plugin

            _pluginRepository.RegisterPlugin(new Plugin
            {
                AssemblyPath = command.AssemblyPath,
                Name = command.PluginName,
                Status = "registered"
            });
        }
    }
}
