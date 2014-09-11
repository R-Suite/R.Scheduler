using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl.Matchers;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using R.Scheduler.JobRunners;

namespace R.Scheduler
{
    /// <summary>
    /// todo: separate core scheduler functionality from the plugin-specific scheduler functionality 
    /// todo: remove dependency on PluginRunner
    /// </summary>
    public class SchedulerCore : ISchedulerCore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginStore;

        public SchedulerCore(IPluginStore pluginStore)
        {
            _pluginStore = pluginStore;
        }

        public void ExecutePlugin(string pluginName)
        {
            var registeredPlugin = _pluginStore.GetRegisteredPlugin(pluginName);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", pluginName);
                return;
            }

            IScheduler sched = Scheduler.Instance();

            // Set default values
            Guid temp = Guid.NewGuid();
            string name = temp + "_Name";
            string group = temp + "_Group";
            string jobName = temp + "_Job";
            string jobGroup = temp + "_JobGroup";

            IJobDetail jobDetail = JobBuilder.Create<PluginRunner>()
                .WithIdentity(jobName, jobGroup)
                .StoreDurably(false)
                .Build();
            jobDetail.JobDataMap.Add("pluginPath", registeredPlugin.AssemblyPath);

            var trigger = (ISimpleTrigger) TriggerBuilder.Create()
                .WithIdentity(name, group)
                .StartNow()
                .ForJob(jobDetail)
                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                .Build();

            sched.ScheduleJob(jobDetail, trigger);
        }

        public void RegisterPlugin(string pluginName, string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Logger.ErrorFormat("Error registering plugin {0}. Invalid assembly path {1}", pluginName, assemblyPath);
                return;
            }
            //todo: verify valid plugin.. reflection?

            _pluginStore.RegisterPlugin(new Plugin
            {
                AssemblyPath = assemblyPath,
                Name = pluginName,
                Status = "registered"
            });
        }

        public void RemovePlugin(string pluginName)
        {
            IScheduler sched = Scheduler.Instance();

            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(pluginName));

            sched.DeleteJobs(jobKeys.ToList());

            int result = _pluginStore.RemovePlugin(pluginName);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing  from data store. Plugin {0} not found", pluginName);
            }
        }

        public void RemoveJobGroup(string groupName)
        {
            IScheduler sched = Scheduler.Instance();

            Quartz.Collection.ISet<JobKey> jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));
            sched.DeleteJobs(jobKeys.ToList());
        }

        public void RemoveJob(string jobName, string jobGroup = null)
        {
            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentException("jobName is null or empty.");

            IScheduler sched = Scheduler.Instance();

            IList<string> jobGroups = !string.IsNullOrEmpty(jobGroup) ? new List<string> { jobGroup } : sched.GetJobGroupNames();

            foreach (string group in jobGroups)
            {
                var jobKey = new JobKey(jobName, group);
                sched.DeleteJob(jobKey);
            }
        }

        public void RemoveTriggerGroup(string groupName)
        {
            IScheduler sched = Scheduler.Instance();

            Quartz.Collection.ISet<TriggerKey> triggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
            sched.UnscheduleJobs(triggerKeys.ToList());
        }

        public void RemoveTrigger(string triggerName, string triggerGroup = null)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentException("triggerName is null or empty.");

            IScheduler sched = Scheduler.Instance();

            IList<string> triggerGroups = !string.IsNullOrEmpty(triggerGroup) ? new List<string> { triggerGroup} : sched.GetTriggerGroupNames();

            foreach (string group in triggerGroups)
            {
                var triggerKey = new TriggerKey(triggerName, group);
                sched.UnscheduleJob(triggerKey);
            }
        }

        public void ScheduleTrigger(BaseTrigger myTrigger)
        {
            IScheduler sched = Scheduler.Instance();

            // Set default values
            Guid temp = Guid.NewGuid();
            string name = !string.IsNullOrEmpty(myTrigger.Name) ? myTrigger.Name : temp + "_Name";
            string group = !string.IsNullOrEmpty(myTrigger.Group) ? myTrigger.Group : temp + "_Group";
            string jobName = !string.IsNullOrEmpty(myTrigger.JobName) ? myTrigger.JobName : temp + "_Job";
            string jobGroup = !string.IsNullOrEmpty(myTrigger.JobGroup) ? myTrigger.JobGroup : temp + "_JobGroup";

            DateTimeOffset startAt = (DateTime.MinValue != myTrigger.StartDateTime) ? myTrigger.StartDateTime : DateTime.UtcNow;

            // Check if jobDetail already exists
            var jobKey = new JobKey(jobName, jobGroup);
            IJobDetail jobDetail = sched.GetJobDetail(jobKey);

            // If jobDetail does not exist, create new
            if (null == jobDetail)
            {
                jobDetail = JobBuilder.Create<PluginRunner>()
                    .WithIdentity(jobName, jobGroup)
                    .StoreDurably(false)
                    .Build();

                foreach (var mapItem in myTrigger.DataMap)
                {
                    jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
                }
            }

            var cronTrigger = myTrigger as CronTrigger;
            if (cronTrigger != null)
            {
                var trigger = (ICronTrigger) TriggerBuilder.Create()
                    .WithIdentity(name, group)
                    .ForJob(jobDetail)
                    .WithCronSchedule(cronTrigger.CronExpression)
                    .StartAt(startAt)
                    .Build();

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

            var simpleTrigger = myTrigger as SimpleTrigger;
            if (simpleTrigger != null)
            {
                var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity(name, group)
                    .ForJob(jobDetail)
                    .StartAt(startAt)
                    .WithSimpleSchedule(x => x
                        .WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount))
                    .Build();

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
}
