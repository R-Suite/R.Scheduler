using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using R.Scheduler.Interfaces;

namespace R.Scheduler
{
    /// <summary>
    /// 
    /// </summary>
    public class SchedulerCore : ISchedulerCore
    {
        private readonly IScheduler _scheduler;

        public string SchedulerName { get { return _scheduler.SchedulerName; } }

        public SchedulerCore(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IEnumerable<IJobDetail> GetJobDetails(Type jobType = null)
        {
            IList<IJobDetail> jobDetails = new List<IJobDetail>();
            IList<string> jobGroups = _scheduler.GetJobGroupNames();

            foreach (string group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var detail = _scheduler.GetJobDetail(jobKey);

                    if (null == jobType)
                    {
                        jobDetails.Add(detail);
                    }
                    else
                    {
                        if (jobType == detail.JobType)
                        {
                            jobDetails.Add(detail);
                        }
                    }
                }
            }

            return jobDetails;
        }

        public void ExecuteJob(string jobName, string groupName)
        {
            groupName = (!string.IsNullOrEmpty(groupName)) ? groupName : JobKey.DefaultGroup;

            var jobKey = new JobKey(jobName, groupName);

            IJobDetail jobDetail = _scheduler.GetJobDetail(jobKey);
            _scheduler.TriggerJob(jobKey);
        }

        public void RemoveJobTriggers(string jobName, string groupName)
        {
            groupName = (!string.IsNullOrEmpty(groupName)) ? groupName : JobKey.DefaultGroup;

            var jobKey = new JobKey(jobName, groupName);

            IList<ITrigger> triggers = _scheduler.GetTriggersOfJob(jobKey);

            foreach (var trigger in triggers)
            {
                _scheduler.UnscheduleJob(trigger.Key);
            }
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

        public void CreateJob(string jobName, string groupName, Type jobType, Dictionary<string, object> dataMap )
        {
            groupName = (!string.IsNullOrEmpty(groupName)) ? groupName : JobKey.DefaultGroup;

            IJobDetail jobDetail = new JobDetailImpl(jobName, groupName, jobType, true, false);
            foreach (var mapItem in dataMap)
            {
                jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
            }
            _scheduler.AddJob(jobDetail, true);
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

        public void ScheduleTrigger(BaseTrigger myTrigger)
        {
            // Set default values
            DateTimeOffset startAt = (DateTime.MinValue != myTrigger.StartDateTime) ? myTrigger.StartDateTime : DateTime.UtcNow;

            // Set default trigger group
            myTrigger.Group = (!string.IsNullOrEmpty(myTrigger.Group)) ? myTrigger.Group : TriggerKey.DefaultGroup;

            // Check if jobDetail already exists
            var jobKey = new JobKey(myTrigger.JobName, myTrigger.JobGroup);

            IJobDetail jobDetail = _scheduler.GetJobDetail(jobKey);

            // If jobDetail does not exist, throw
            if (null == jobDetail)
            {
                throw new Exception(string.Format("Job does not exist. Name = {0}, Group = {1}", myTrigger.JobName, myTrigger.JobGroup));
            }

            var cronTrigger = myTrigger as CronTrigger;
            if (cronTrigger != null)
            {
                var trigger = (ICronTrigger)TriggerBuilder.Create()
                    .WithIdentity(myTrigger.Name, myTrigger.Group)
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
                    .WithIdentity(myTrigger.Name, myTrigger.Group)
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

        public IEnumerable<ITrigger> GetTriggersOfJobGroup(string jobGroup)
        {
            var retval = new List<ITrigger>();

            Quartz.Collection.ISet<JobKey> jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGroup));

            foreach (var jobKey in jobKeys)
            {
                retval.AddRange(_scheduler.GetTriggersOfJob(jobKey));
            }

            return retval;
        }

        public IEnumerable<ITrigger> GetTriggersOfJob(string jobName, string jobGroup = null)
        {
            // Set default trigger group
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            return _scheduler.GetTriggersOfJob(new JobKey(jobName, jobGroup));
        }
    }
}
