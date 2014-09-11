using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class DescheduleTriggerGroupHandler : IMessageHandler<DescheduleTriggerGroup>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DescheduleTriggerGroupHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(DescheduleTriggerGroup message)
        {
            Logger.InfoFormat("Entered DescheduleTriggerGroupHandler.Execute(). TriggerGroup = {0}", message.TriggerGroup);

            _schedulerCore.RemoveTriggerGroup(message.TriggerGroup);
        }

        public IConsumeContext Context { get; set; }
    }
}
