using System.Linq;
using Quartz;
using Quartz.Impl.Matchers;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class DescheduleJobHandler : IMessageHandler<DescheduleJob>
    {
        public void Execute(DescheduleJob message)
        {
            IScheduler sched = Scheduler.Instance();

            string groupName = !string.IsNullOrEmpty(message.GroupName) ? message.GroupName : message.PluginName;

            // trigger name is provided, delete specific trigger
            if (!string.IsNullOrEmpty(message.TriggerName))
            {
                var triggerKey = new TriggerKey(message.TriggerName, groupName);
                sched.UnscheduleJob(triggerKey);
            }
            // trigger name is not provided, delete all plugin triggers
            else
            {
                Quartz.Collection.ISet<TriggerKey> triggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
                sched.UnscheduleJobs(triggerKeys.ToList());
            }

            // delete job if no triggers are left
            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));

            foreach (var jobKey in jobKeys)
            {
                var triggers = sched.GetTriggersOfJob(jobKey);

                if (!triggers.Any())
                    sched.DeleteJob(jobKey);
            }
        }
    }
}
