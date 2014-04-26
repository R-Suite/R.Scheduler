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
            var registeredPlugin = _pluginStore.GetRegisteredPlugin(command.PluginName);

            if (null == registeredPlugin)
            {
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", command.PluginName));
            }

            // Set default values
            string groupName = !string.IsNullOrEmpty(command.GroupName) ? command.GroupName : command.PluginName;
            string jobName = !string.IsNullOrEmpty(command.JobName) ? command.JobName : command.PluginName + "_Job_" + DateTime.UtcNow.ToString("yyMMddhhmmss");
            string triggerName = !string.IsNullOrEmpty(command.TriggerName) ? command.TriggerName : command.PluginName + "_Trigger_" + DateTime.UtcNow.ToString("yyMMddhhmmss");
            DateTimeOffset startAt = (DateTime.MinValue != command.StartDateTime) ? command.StartDateTime : DateTime.UtcNow;

            IJobDetail jobDetail = JobBuilder.Create<PluginRunner>()
                .WithIdentity(jobName, groupName)
                .StoreDurably(false)
                .Build();

            jobDetail.JobDataMap.Add("pluginPath", registeredPlugin.AssemblyPath);

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
                .StartAt(startAt)
                .Build();

            trigger.RepeatCount = command.RepeatCount;
            trigger.RepeatInterval = command.RepeatInterval;

            IScheduler sched = Scheduler.Instance();
            sched.ScheduleJob(jobDetail, trigger);
        }
    }
}
