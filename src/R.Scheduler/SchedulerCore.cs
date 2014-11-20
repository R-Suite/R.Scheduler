using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl.Matchers;
using R.Scheduler.Interfaces;

namespace R.Scheduler
{
    /// <summary>
    /// 
    /// </summary>
    public class SchedulerCore : ISchedulerCore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IScheduler _scheduler;

        public SchedulerCore(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void ExecuteJob(Type jobType, Dictionary<string, object> dataMap)
        {
            // Set default values
            Guid temp = Guid.NewGuid();
            string name = temp + "_Name";
            string group = temp + "_Group";
            string jobName = temp + "_Job";
            string jobGroup = temp + "_JobGroup";

            IJobDetail jobDetail = JobBuilder.Create(jobType)
                .WithIdentity(jobName, jobGroup)
                .StoreDurably(false)
                .Build();

            if (null != dataMap && dataMap.Count > 0)
            {
                foreach (var mapItem in dataMap)
                {
                    jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
                }
            }

            var trigger = (ISimpleTrigger) TriggerBuilder.Create()
                .WithIdentity(name, group)
                .StartNow()
                .ForJob(jobDetail)
                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                .Build();

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void RemoveJobGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName is null or empty.");

            Quartz.Collection.ISet<JobKey> jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));
            _scheduler.DeleteJobs(jobKeys.ToList());
        }

        public void RemoveJob(string jobName, string jobGroup = null)
        {
            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentException("jobName is null or empty.");

            IList<string> jobGroups = !string.IsNullOrEmpty(jobGroup) ? new List<string> { jobGroup } : _scheduler.GetJobGroupNames();

            foreach (string group in jobGroups)
            {
                var jobKey = new JobKey(jobName, group);

                if (_scheduler.CheckExists(jobKey))
                    _scheduler.DeleteJob(jobKey);
            }
        }

        public void RemoveTriggerGroup(string groupName)
        {
            Quartz.Collection.ISet<TriggerKey> triggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
            _scheduler.UnscheduleJobs(triggerKeys.ToList());
        }

        public void RemoveTrigger(string triggerName, string triggerGroup = null)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentException("triggerName is null or empty.");

            IList<string> triggerGroups = !string.IsNullOrEmpty(triggerGroup) ? new List<string> { triggerGroup } : _scheduler.GetTriggerGroupNames();

            foreach (string group in triggerGroups)
            {
                var triggerKey = new TriggerKey(triggerName, group);

                if (_scheduler.CheckExists(triggerKey))
                    _scheduler.UnscheduleJob(triggerKey);
            }
        }

        public void ScheduleTrigger(BaseTrigger myTrigger, Type jobType)
        {
            // Set default values
            Guid temp = Guid.NewGuid();
            string name = !string.IsNullOrEmpty(myTrigger.Name) ? myTrigger.Name : temp + "_Name";
            string group = !string.IsNullOrEmpty(myTrigger.Group) ? myTrigger.Group : temp + "_Group";
            string jobName = !string.IsNullOrEmpty(myTrigger.JobName) ? myTrigger.JobName : temp + "_Job";
            string jobGroup = !string.IsNullOrEmpty(myTrigger.JobGroup) ? myTrigger.JobGroup : temp + "_JobGroup";

            DateTimeOffset startAt = (DateTime.MinValue != myTrigger.StartDateTime) ? myTrigger.StartDateTime : DateTime.UtcNow;

            // Check if jobDetail already exists
            var jobKey = new JobKey(jobName, jobGroup);
            IJobDetail jobDetail = _scheduler.GetJobDetail(jobKey);

            // If jobDetail does not exist, create new
            if (null == jobDetail)
            {
                jobDetail = JobBuilder.Create(jobType)
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
                if (_scheduler.CheckExists(jobKey))
                {
                    _scheduler.ScheduleJob(trigger);
                }
                else
                {
                    _scheduler.ScheduleJob(jobDetail, trigger);
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
                if (_scheduler.CheckExists(jobKey))
                {
                    _scheduler.ScheduleJob(trigger);
                }
                else
                {
                    _scheduler.ScheduleJob(jobDetail, trigger);
                }
            }
        }

        public IList<ITrigger> GetTriggersOfJobGroup(string groupName)
        {
            var retval = new List<ITrigger>();

            Quartz.Collection.ISet<JobKey> jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));

            foreach (var jobKey in jobKeys)
            {
                retval.AddRange(_scheduler.GetTriggersOfJob(jobKey));
            }

            return retval;
        }

        public IList<ITrigger> GetTriggersOfJobType(Type jobType)
        {
            var retval = new List<ITrigger>();

            Quartz.Collection.ISet<JobKey> jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            foreach (var jobKey in jobKeys)
            {
                IJobDetail jobDetails = _scheduler.GetJobDetail(jobKey);

                if (jobDetails.JobType == jobType)
                {
                    retval.AddRange(_scheduler.GetTriggersOfJob(jobDetails.Key));
                }
            }

            return retval;
        }

        public void RemoveTriggersOfJobType(Type jobType)
        {
            Quartz.Collection.ISet<JobKey> jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            foreach (var jobKey in jobKeys)
            {
                IJobDetail jobDetails = _scheduler.GetJobDetail(jobKey);

                if (jobDetails.JobType == jobType)
                {
                    _scheduler.DeleteJob(jobDetails.Key);
                }
            }
            
        }
    }
}
