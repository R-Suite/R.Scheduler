using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class RemovePluginHandler : IMessageHandler<RemovePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public RemovePluginHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(RemovePlugin message)
        {
            Logger.InfoFormat("Entered RemovePluginHandler.Execute(). PluginName = {0}", message.PluginName);

            _schedulerCore.RemovePlugin(message.PluginName);
        }

        public IConsumeContext Context { get; set; }
    }
}
