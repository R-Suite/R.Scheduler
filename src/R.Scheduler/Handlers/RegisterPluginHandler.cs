using System.IO;
using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class RegisterPluginHandler : IMessageHandler<RegisterPlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;

        public RegisterPluginHandler(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Execute(RegisterPlugin command)
        {
            if (!File.Exists(command.AssemblyPath))
            {
                Logger.ErrorFormat("Error registering plugin. Ivalid assembly path {0}", command.AssemblyPath);
                return;
            }
            //todo: verify valid plugin.. reflection?

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
