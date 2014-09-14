using System.Collections.Generic;
using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class ExecutePluginHandler : IMessageHandler<ExecutePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;
        private readonly IPluginStore _pluginStore;

        public ExecutePluginHandler(ISchedulerCore schedulerCore, IPluginStore pluginStore)
        {
            _schedulerCore = schedulerCore;
            _pluginStore = pluginStore;
        }

        public void Execute(ExecutePlugin message)
        {
            Logger.InfoFormat("Entered ExecutePluginHandler.Execute(). PluginName = {0}", message.PluginName);

            var registeredPlugin = _pluginStore.GetRegisteredPlugin(message.PluginName);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", message.PluginName);
                return;
            }

            var dataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } };

            _schedulerCore.ExecuteJob(typeof(PluginRunner), dataMap);
        }

        public IConsumeContext Context { get; set; }
    }
}
