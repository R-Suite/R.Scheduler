using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class RegisterPluginHandler : IMessageHandler<RegisterPlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;
        private readonly IJobTypeManager _pluginManager;

        public RegisterPluginHandler(ISchedulerCore schedulerCore, IJobTypeManager pluginManager)
        {
            _schedulerCore = schedulerCore;
            _pluginManager = pluginManager;
        }

        public void Execute(RegisterPlugin command)
        {
            Logger.InfoFormat("Entered RegisterPluginHandler.Execute(). PluginName = {0}", command.PluginName);

            _pluginManager.Register(command.PluginName, command.AssemblyPath);
        }

        public IConsumeContext Context { get; set; }
    }
}
