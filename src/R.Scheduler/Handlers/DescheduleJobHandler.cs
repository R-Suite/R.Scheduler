using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Handlers
{
    public class DescheduleJobHandler : IMessageHandler<DescheduleJob>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DescheduleJobHandler(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        public void Execute(DescheduleJob message)
        {
            Logger.InfoFormat("Entered DescheduleJobHandler.Execute(). JobName = {0}", message.JobName);

            _schedulerCore.RemoveJob(message.JobName);
        }

        public IConsumeContext Context { get; set; }
    }
}
