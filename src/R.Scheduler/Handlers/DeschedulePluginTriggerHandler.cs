using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class DeschedulePluginTriggerHandler : IMessageHandler<DeschedulePluginTrigger>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DeschedulePluginTriggerHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(DeschedulePluginTrigger message)
        {
            Logger.InfoFormat("Entered DeschedulePluginTriggerHandler.Execute(). PluginName = {0}. TriggerName = {1}", message.PluginName, message.TriggerName);

            _schedulerCore.DescheduleTrigger(message.PluginName, message.TriggerName);
        }

        public IConsumeContext Context { get; set; }
    }
}
