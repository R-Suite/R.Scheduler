using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class RegisterPluginCommandHandler : IMessageHandler<RegisterPluginCommand>
    {
        readonly IPluginRepository _pluginRepository;

        public RegisterPluginCommandHandler(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(RegisterPluginCommand command)
        {
            //todo: verify valid plugin

            _pluginRepository.RegisterPlugin(new RegisteredPlugin
            {
                AssemblyPath = command.AssemblyPath,
                Name = command.PluginName,
                Status = "registered"
            });
        }
    }
}
