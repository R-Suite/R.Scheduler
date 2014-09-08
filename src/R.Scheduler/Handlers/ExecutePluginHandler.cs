using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class ExecutePluginHandler : IMessageHandler<ExecutePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public ExecutePluginHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(ExecutePlugin message)
        {
            Logger.InfoFormat("Entered ExecutePluginHandler.Execute(). PluginName = {0}", message.PluginName);

            _schedulerCore.ExecutePlugin(message.PluginName);
        }

        public IConsumeContext Context { get; set; }
    }
}
