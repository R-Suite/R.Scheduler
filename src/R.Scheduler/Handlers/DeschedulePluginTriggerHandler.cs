using System;
using System.Linq;
using Quartz;
using Quartz.Impl.Matchers;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class DeschedulePluginTriggerHandler : IMessageHandler<DeschedulePluginTrigger>
    {
        public void Execute(DeschedulePluginTrigger message)
        {
            if (string.IsNullOrEmpty(message.PluginName) || string.IsNullOrEmpty(message.TriggerName))
                throw new ArgumentException("One or both of the required fields (PluginName, TriggerName) is null or empty.");

            IScheduler sched = Scheduler.Instance();

            string groupName = message.PluginName;
            string triggerName = message.TriggerName;

            var triggerKey = new TriggerKey(triggerName, groupName);
            sched.UnscheduleJob(triggerKey);

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
