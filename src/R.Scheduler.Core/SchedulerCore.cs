using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
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

        /// <summary>
        /// Get JobDetail
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public IJobDetail GetJobDetail(string jobName, string jobGroup = null)
        {
            // Use DefaultGroup if jobGroup is null or empty
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            var jobKey = new JobKey(jobName, jobGroup);

            return _scheduler.GetJobDetail(jobKey);
        }

        /// <summary>
        /// Trigger job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void ExecuteJob(string jobName, string jobGroup)
        {
            // Use DefaultGroup if jobGroup is null or empty
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            var jobKey = new JobKey(jobName, jobGroup);

            _scheduler.TriggerJob(jobKey);
        }

        /// <summary>
        /// Removes all triggers of the specified job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void RemoveJobTriggers(string jobName, string jobGroup)
        {
            // Use DefaultGroup if jobGroup is null or empty
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            var jobKey = new JobKey(jobName, jobGroup);

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

        /// <summary>
        /// Create new job of type <see cref="jobType"/> without any triggers
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobType"></param>
        /// <param name="dataMap"><see cref="jobType"/> specific parameters</param>
        public void CreateJob(string jobName, string jobGroup, Type jobType, Dictionary<string, object> dataMap )
        {
            // Use DefaultGroup if jobGroup is null or empty
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            IJobDetail jobDetail = new JobDetailImpl(jobName, jobGroup, jobType, true, false);
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

        /// <summary>
        /// Remove job and all associated triggers.
        /// Assume JobKey.DefaultGroup if jobGroup not provided.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void RemoveJob(string jobName, string jobGroup = null)
        {
            if (string.IsNullOrEmpty(jobName))
                throw new ArgumentException("jobName is null or empty.");

            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;
            var jobKey = new JobKey(jobName, jobGroup);

            if (_scheduler.CheckExists(jobKey))
            {
                _scheduler.DeleteJob(jobKey);
            }
            else
            {
                throw new KeyNotFoundException(string.Format("JobKey not found for {0}, {1}", jobName, jobGroup));
            }
        }

        /// <summary>
        /// Remove trigger from scheduler.
        /// Assume TriggerKey.DefaultGroup if triggerGroup not provided.
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        public void RemoveTrigger(string triggerName, string triggerGroup = null)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentException("triggerName is null or empty.");

            triggerGroup = (!string.IsNullOrEmpty(triggerGroup)) ? triggerGroup : TriggerKey.DefaultGroup;

            var triggerKey = new TriggerKey(triggerName, triggerGroup);

            if (_scheduler.CheckExists(triggerKey))
            {
                _scheduler.UnscheduleJob(triggerKey);
            }
            else
            {
                throw new KeyNotFoundException(string.Format("TriggerKey not found for {0}, {1}", triggerName, triggerGroup));
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

        /// <summary>
        /// Schedule specified trigger
        /// </summary>
        /// <param name="myTrigger"></param>
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
