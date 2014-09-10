using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Job;
using Quartz.Spi;
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

            IJobDetail jobDetail = new JobDetailImpl { JobDataMap = new JobDataMap { { "pluginPath", registeredPlugin.AssemblyPath} } };
            IOperableTrigger trigger = new SimpleTriggerImpl("AdHockTrigger") { RepeatCount = 1};
            var tfb = new TriggerFiredBundle(jobDetail, trigger, null, false, null, null, null, null);

            IJob job = new NoOpJob();

            IJobExecutionContext context = new JobExecutionContextImpl(null, tfb, job);

            var pluginRunner = new PluginRunner();

            pluginRunner.Execute(context);
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

            foreach (var jobKey in jobKeys)
            {
                sched.DeleteJob(jobKey);
            }

            int result = _pluginStore.RemovePlugin(pluginName);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing  from data store. Plugin {0} not found", pluginName);
            }
        }

        public void DescheduleGroup(string groupName)
        {
            IScheduler sched = Scheduler.Instance();

            Quartz.Collection.ISet<TriggerKey> triggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
            sched.UnscheduleJobs(triggerKeys.ToList());

            // delete job if no triggers are left
            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));

            foreach (var jobKey in jobKeys)
            {
                sched.DeleteJob(jobKey);
            }
        }

        public void DescheduleTrigger(string groupName, string triggerName)
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(triggerName))
                throw new ArgumentException("One or both of the required fields (groupName, triggerName) is null or empty.");

            IScheduler sched = Scheduler.Instance();

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

        public void ScheduleSimpleTrigger(SimpleTrigger simpleTrigger)
        {
            if (string.IsNullOrEmpty(simpleTrigger.GroupName))
                throw new ArgumentException("Required fields GroupName is null or empty.");

            IScheduler sched = Scheduler.Instance();

            // Set default values
            string groupName = simpleTrigger.GroupName;
            string jobName = !string.IsNullOrEmpty(simpleTrigger.JobName) ? simpleTrigger.JobName : "Job_" + simpleTrigger.GroupName;
            string triggerName = !string.IsNullOrEmpty(simpleTrigger.TriggerName) ? simpleTrigger.TriggerName : simpleTrigger.GroupName + "_Trigger_" + DateTime.UtcNow.ToString("yyMMddhhmmss");
            DateTimeOffset startAt = (DateTime.MinValue != simpleTrigger.StartDateTime) ? simpleTrigger.StartDateTime : DateTime.UtcNow;

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
                foreach (var mapItem in simpleTrigger.DataMap)
                {
                    jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
                }
            }

            // Create new "Simple" Trigger
            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
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

        public void ScheduleCronTrigger(CronTrigger cronTrigger)
        {
            if (string.IsNullOrEmpty(cronTrigger.Name))
                throw new ArgumentException("Required fields Name is null or empty.");

            IScheduler sched = Scheduler.Instance();

            // Set default values
            string name = cronTrigger.Name;
            string group = !string.IsNullOrEmpty(cronTrigger.Group) ? cronTrigger.Group : cronTrigger.Name + "_TriggerGroup_" + DateTime.UtcNow.ToString("yyMMddhhmmss");
            string jobName = !string.IsNullOrEmpty(cronTrigger.JobName) ? cronTrigger.Name : "Job_" + cronTrigger.Name;
            string jobGroup = !string.IsNullOrEmpty(cronTrigger.JobGroup) ? cronTrigger.Name : "JobGroup_" + cronTrigger.Name;
            DateTimeOffset startAt = (DateTime.MinValue != cronTrigger.StartDateTime) ? cronTrigger.StartDateTime : DateTime.UtcNow;

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
                foreach (var mapItem in cronTrigger.DataMap)
                {
                    jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
                }
            }

            // Create new "Simple" Trigger
            var trigger = (ICronTrigger)TriggerBuilder.Create()
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
    }
}
