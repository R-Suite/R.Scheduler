using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Provides abstraction layer between the Quartz Scheduler 
    /// and the R.Scheduler controllers
    /// </summary>
    public class SchedulerCore : ISchedulerCore
    {
        private readonly IScheduler _scheduler;

        public string SchedulerName { get { return _scheduler.SchedulerName; } }

        public SchedulerCore(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        /// <summary>
        /// Get job details of type <see cref="jobType"/>.
        /// Get all the job details if jobType is not specified
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
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
        /// Get JobDetail of the specified job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public IJobDetail GetJobDetail(string jobName, string jobGroup)
        {
            var jobKey = new JobKey(jobName, jobGroup);

            return _scheduler.GetJobDetail(jobKey);
        }

        /// <summary>
        /// Trigger the specified job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void ExecuteJob(string jobName, string jobGroup)
        {
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
            var jobKey = new JobKey(jobName, jobGroup);

            IList<ITrigger> triggers = _scheduler.GetTriggersOfJob(jobKey);

            foreach (var trigger in triggers)
            {
                _scheduler.UnscheduleJob(trigger.Key);
            }
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

        /// <summary>
        /// Remove job and all associated triggers.
        /// Assume JobKey.DefaultGroup if jobGroup not provided.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        public void RemoveJob(string jobName, string jobGroup)
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
        public void RemoveTrigger(string triggerName, string triggerGroup)
        {
            if (string.IsNullOrEmpty(triggerName))
                throw new ArgumentException("triggerName is null or empty.");

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

            // If jobDetail does not exist, throw
            if (!_scheduler.CheckExists(jobKey))
            {
                throw new Exception(string.Format("Job does not exist. Name = {0}, Group = {1}", myTrigger.JobName, myTrigger.JobGroup));
            }

            IJobDetail jobDetail = _scheduler.GetJobDetail(jobKey);

            var cronTrigger = myTrigger as CronTrigger;
            if (cronTrigger != null)
            {
                var trigger = (ICronTrigger)TriggerBuilder.Create()
                    .WithIdentity(myTrigger.Name, myTrigger.Group)
                    .ForJob(jobDetail)
                    .WithCronSchedule(cronTrigger.CronExpression)
                    .StartAt(startAt)
                    .Build();

                _scheduler.ScheduleJob(trigger);
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

                _scheduler.ScheduleJob(trigger);
            }
        }

        /// <summary>
        /// Get all triggers of a specified job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public IEnumerable<ITrigger> GetTriggersOfJob(string jobName, string jobGroup)
        {
            return _scheduler.GetTriggersOfJob(new JobKey(jobName, jobGroup));
        }
    }
}
