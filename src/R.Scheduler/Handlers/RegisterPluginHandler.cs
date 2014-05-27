using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class RegisterPluginHandler : IMessageHandler<RegisterPlugin>
    {
        readonly IPluginStore _pluginRepository;

        public RegisterPluginHandler(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(RegisterPlugin command)
        {
            //todo: verify valid plugin

            _pluginRepository.RegisterPlugin(new Plugin
            {
                AssemblyPath = command.AssemblyPath,
                Name = command.PluginName,
                Status = "registered"
            });
        }

        public IConsumeContext Context { get; set; }
    }
}
