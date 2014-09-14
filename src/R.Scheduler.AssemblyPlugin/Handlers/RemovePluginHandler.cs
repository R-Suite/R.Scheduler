using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class RemovePluginHandler : IMessageHandler<RemovePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;
        private readonly IPluginStore _pluginRepository;

        public RemovePluginHandler(ISchedulerCore schedulerCore, IPluginStore pluginRepository)
        {
            _schedulerCore = schedulerCore;
            _pluginRepository = pluginRepository;
        }

        public void Execute(RemovePlugin message)
        {
            Logger.InfoFormat("Entered RemovePluginHandler.Execute(). PluginName = {0}", message.PluginName);

            _schedulerCore.RemoveJobGroup(message.PluginName);

            int result = _pluginRepository.RemovePlugin(message.PluginName);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing from data store. Plugin {0} not found", message.PluginName);
            }
        }

        public IConsumeContext Context { get; set; }
    }
}
