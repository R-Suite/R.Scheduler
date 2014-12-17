using System;
using System.Collections;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using R.Scheduler.Contracts.Model;
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
        /// Get all the job details if <see cref="jobType"/> is not specified
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
        /// Get <see cref="IJobDetail"/> of the specified job
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
        /// Assume <see cref="JobKey.DefaultGroup"/> if jobGroup not provided.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <exception cref="ArgumentException"> 
        /// If the jobName is an empty string.
        /// </exception>
        /// /// <exception cref="KeyNotFoundException"> 
        /// If the jobKey is not found.
        /// </exception>
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
        /// Assume <see cref="TriggerKey.DefaultGroup"/> if triggerGroup not provided.
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
                throw new Exception(string.Format("Job does not exist. Name = {0}, Group = {1}", myTrigger.CalendarName, myTrigger.JobGroup));
            }

            IJobDetail jobDetail = _scheduler.GetJobDetail(jobKey);

            var cronTrigger = myTrigger as CronTrigger;
            if (cronTrigger != null)
            {
                //SmartPolicy
                Action<CronScheduleBuilder> misFireAction = x => {};

                switch (cronTrigger.MisfireInstruction)
                {
                    case MisfireInstructionCron.DoNothing:
                        misFireAction = builder => builder.WithMisfireHandlingInstructionDoNothing();
                        break;
                    case MisfireInstructionCron.FireOnceNow:
                        misFireAction = builder => builder.WithMisfireHandlingInstructionFireAndProceed();
                        break;
                    case MisfireInstructionCron.Ignore:
                        misFireAction = builder => builder.WithMisfireHandlingInstructionIgnoreMisfires();
                        break;
                }

                var trigger = (ICronTrigger)TriggerBuilder.Create()
                    .WithIdentity(myTrigger.Name, myTrigger.Group)
                    .ForJob(jobDetail)
                    .ModifiedByCalendar(!string.IsNullOrEmpty(cronTrigger.CalendarName) ? cronTrigger.CalendarName : null)
                    .WithCronSchedule(cronTrigger.CronExpression, misFireAction)
                    .StartAt(startAt)
                    .Build();

                _scheduler.ScheduleJob(trigger);
            }

            var simpleTrigger = myTrigger as SimpleTrigger;
            if (simpleTrigger != null)
            {
                //SmartPolicy
                Action<SimpleScheduleBuilder> misFireAction = x => x.WithInterval(simpleTrigger.RepeatInterval)
                    .WithRepeatCount(simpleTrigger.RepeatCount);

                switch (simpleTrigger.MisfireInstruction)
                {
                    case MisfireInstructionSimple.FireNow:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionFireNow();
                        break;
                    case MisfireInstructionSimple.Ignore:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionIgnoreMisfires();
                        break;
                    case MisfireInstructionSimple.RescheduleNextWithExistingCount:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionNextWithExistingCount();
                        break;
                    case MisfireInstructionSimple.RescheduleNextWithRemainingCount:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionNextWithRemainingCount();
                        break;
                    case MisfireInstructionSimple.RescheduleNowWithExistingRepeatCount:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionNowWithExistingCount();
                        break;
                    case MisfireInstructionSimple.RescheduleNowWithRemainingRepeatCount:
                        misFireAction = builder => builder.WithInterval(simpleTrigger.RepeatInterval)
                        .WithRepeatCount(simpleTrigger.RepeatCount).WithMisfireHandlingInstructionNowWithRemainingCount();
                        break;
                }

                var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity(myTrigger.Name, myTrigger.Group)
                    .ForJob(jobDetail)
                    .ModifiedByCalendar(!string.IsNullOrEmpty(simpleTrigger.CalendarName) ? simpleTrigger.CalendarName : null)
                    .StartAt(startAt)
                    .WithSimpleSchedule(misFireAction)
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

        /// <summary>
        /// Get all the fire times within a specified date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<TriggerFireTime> GetFireTimesBetween(DateTime start, DateTime end)
        {
            IList<TriggerFireTime> retval = new List<TriggerFireTime>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys)
            {
                ITrigger trigger = _scheduler.GetTrigger(triggerKey);
                ICalendar cal = null;
                if (!string.IsNullOrEmpty(trigger.CalendarName))
                {
                    cal = _scheduler.GetCalendar(trigger.CalendarName);
                }
                var fireTimes = TriggerUtils.ComputeFireTimesBetween(trigger as IOperableTrigger, cal, start, end);

                foreach (var fireTime in fireTimes)
                {
                    retval.Add(new TriggerFireTime
                    {
                        FireDateTime = fireTime,
                        JobName = trigger.JobKey.Name,
                        JobGroup = trigger.JobKey.Group,
                        TriggerName = trigger.Key.Name,
                        TriggerGroup = trigger.Key.Group
                    });
                }
            }

            return retval;
        }

        /// <summary>
        /// Register new <see cref="HolidayCalendar"/> and optionally provide an initital set of dates to exclude.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="daysExcludedUtc"></param>
        public void AddHolidayCalendar(string name, string description, IList<DateTime> daysExcludedUtc = null)
        {
            var holidays = new HolidayCalendar();
            holidays.Description = description;

            if (null != daysExcludedUtc && daysExcludedUtc.Count > 0)
            {
                foreach (var dateTime in daysExcludedUtc)
                {
                    holidays.AddExcludedDate(dateTime);
                }
            }

            _scheduler.AddCalendar(name, holidays, true, true);
        }

        /// <summary>
        /// Add exclusion dates to <see cref="HolidayCalendar"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="daysExcludedUtc"></param>
        public void AddHolidayCalendarExclusionDates(string name, IList<DateTime> daysExcludedUtc)
        {
            var holidays = (HolidayCalendar)_scheduler.GetCalendar(name);

            if (null != daysExcludedUtc && daysExcludedUtc.Count > 0)
            {
                foreach (var dateTime in daysExcludedUtc)
                {
                    holidays.AddExcludedDate(dateTime);
                }
            }

            _scheduler.AddCalendar(name, holidays, true, true);
        }

        /// <summary>
        /// Register new <see cref="CronCalendar"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="cronExpression"></param>
        public void AddCronCalendar(string name, string description, string cronExpression)
        {
            var cronCal = new CronCalendar(cronExpression) {Description = description};

            _scheduler.AddCalendar(name, cronCal, true, true);
        }

        /// <summary>
        /// Delete Calendar
        /// </summary>
        /// <param name="name"></param>
        public bool DeleteCalendar(string name)
        {
            return _scheduler.DeleteCalendar(name);
        }
    }
}
