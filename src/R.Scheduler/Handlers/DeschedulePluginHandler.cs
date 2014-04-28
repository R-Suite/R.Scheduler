using System.Linq;
using Quartz;
using Quartz.Impl.Matchers;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class DeschedulePluginHandler : IMessageHandler<DeschedulePlugin>
    {
        public void Execute(DeschedulePlugin message)
        {
            IScheduler sched = Scheduler.Instance();

            Quartz.Collection.ISet<TriggerKey> triggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(message.PluginName));
            sched.UnscheduleJobs(triggerKeys.ToList());

            // delete job if no triggers are left
            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(message.PluginName));

            foreach (var jobKey in jobKeys)
            {
                sched.DeleteJob(jobKey);
            }
        }
    }
}
