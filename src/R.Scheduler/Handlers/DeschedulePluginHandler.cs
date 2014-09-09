using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class DeschedulePluginHandler : IMessageHandler<DeschedulePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DeschedulePluginHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(DeschedulePlugin message)
        {
            Logger.InfoFormat("Entered DeschedulePlugin.Execute(). PluginName = {0}", message.PluginName);

            _schedulerCore.DeschedulePlugin(message.PluginName);
        }

        public IConsumeContext Context { get; set; }
    }
}
