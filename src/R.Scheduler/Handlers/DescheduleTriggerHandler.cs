using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class DescheduleTriggerHandler : IMessageHandler<DescheduleTrigger>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DescheduleTriggerHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(DescheduleTrigger message)
        {
            Logger.InfoFormat("Entered DescheduleTriggerHandler.Execute(). TriggerName = {0}, TriggerGroup = {1}", message.TriggerName, message.TriggerGroup);

            _schedulerCore.RemoveTrigger(message.TriggerName, message.TriggerGroup);
        }

        public IConsumeContext Context { get; set; }
    }
}
