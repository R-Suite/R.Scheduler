using System;
using Quartz;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.JobRunners;

namespace R.Scheduler.Handlers
{
    public class SchedulePluginWithSimpleTriggerHandler : IMessageHandler<SchedulePluginWithSimpleTrigger>
    {
        readonly IPluginStore _pluginStore;

        public SchedulePluginWithSimpleTriggerHandler(IPluginStore pluginStore)
        {
            _pluginStore = pluginStore;
        }

        public void Execute(SchedulePluginWithSimpleTrigger command)
        {
            if (string.IsNullOrEmpty(command.PluginName))
                throw new ArgumentException("Required fields PluginName is null or empty.");

            var registeredPlugin = _pluginStore.GetRegisteredPlugin(command.PluginName);

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", command.PluginName));

            IScheduler sched = Scheduler.Instance();

            // Set default values
            string groupName = command.PluginName;
            string jobName = "Job_" + command.PluginName;
            string triggerName = !string.IsNullOrEmpty(command.TriggerName) ? command.TriggerName : command.PluginName + "_Trigger_" + DateTime.UtcNow.ToString("yyMMddhhmmss");
            DateTimeOffset startAt = (DateTime.MinValue != command.StartDateTime) ? command.StartDateTime : DateTime.UtcNow;

            // Check if jobDetail already exists
            var jobKey = new JobKey(jobName, groupName);
            IJobDetail jobDetail = sched.GetJobDetail(jobKey);

            // If jobDetail does not exist, create new
            if (null == jobDetail)
            {
                jobDetail = JobBuilder.Create<PluginRunner>()
                    .WithIdentity(jobName, groupName)
                    .StoreDurably(false)
                    .Build();

                jobDetail.JobDataMap.Add("pluginPath", registeredPlugin.AssemblyPath);
            }

            // Create new "Simple" Trigger
            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
                .ForJob(jobDetail)
                .StartAt(startAt)
                .Build();

            trigger.RepeatCount = command.RepeatCount;
            trigger.RepeatInterval = command.RepeatInterval;

            // Schedule Job
            if (sched.CheckExists(jobKey))
            {
                sched.ScheduleJob(trigger);
            }
            else
            {
                sched.ScheduleJob(jobDetail, trigger);
            }
        }
    }
}
