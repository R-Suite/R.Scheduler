using System;
using System.Collections.Generic;
using System.Transactions;
using Quartz;
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
        private readonly IPersistanceStore _persistanceStore;

        public string SchedulerName { get { return _scheduler.SchedulerName; } }

        public SchedulerCore(IScheduler scheduler, IPersistanceStore persistanceStore)
        {
            _scheduler = scheduler;
            _persistanceStore = persistanceStore;
        }

        /// <summary>
        /// Get job details of type <see cref="jobType"/>.
        /// Get all the job details if <see cref="jobType"/> is not specified
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public IDictionary<IJobDetail, Guid> GetJobDetails(Type jobType = null)
        {
            IDictionary<IJobDetail, Guid> jobDetails = new Dictionary<IJobDetail, Guid>();
            IList<string> jobGroups = _scheduler.GetJobGroupNames();

            foreach (string group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupEquals(group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var jobId = _persistanceStore.GetJobId(jobKey);
                    var detail = _scheduler.GetJobDetail(jobKey);

                    if (null == jobType)
                    {
                        jobDetails.Add(detail, jobId);
                    }
                    else
                    {
                        if (jobType == detail.JobType)
                        {
                            jobDetails.Add(detail, jobId);
                        }
                    }
                }
            }

            return jobDetails;
        }

        /// <summary>
        /// Get <see cref="IJobDetail"/> of the job specified by id.
        /// This method is used mainly for requests coming from the WebApi.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IJobDetail GetJobDetail(Guid id)
        {
            var jobKey = _persistanceStore.GetJobKey(id);

            return _scheduler.GetJobDetail(jobKey);
        }

        /// <summary>
        /// Trigger the specified job
        /// </summary>
        /// <param name="jobId"></param>
        public void ExecuteJob(Guid jobId)
        {
            var jobKey = _persistanceStore.GetJobKey(jobId);

            _scheduler.TriggerJob(jobKey);
        }

        /// <summary>
        /// Removes all triggers of the specified job
        /// </summary>
        /// <param name="jobId"></param>
        public void RemoveJobTriggers(Guid jobId)
        {
            var jobKey = _persistanceStore.GetJobKey(jobId);

            IList<ITrigger> triggers = _scheduler.GetTriggersOfJob(jobKey);

            foreach (var trigger in triggers)
            {
                _scheduler.UnscheduleJob(trigger.Key);

                // remove trigger key id map entry
                _persistanceStore.RemoveTriggerKeyIdMap(trigger.Key.Name, trigger.Key.Group);
            }
        }

        /// <summary>
        /// Create new job of type <see cref="jobType"/> without any triggers
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobType"></param>
        /// <param name="dataMap"><see cref="jobType"/> specific parameters</param>
        /// <param name="description"></param>
        /// <param name="jobId"></param>
        public Guid CreateJob(string jobName, string jobGroup, Type jobType, Dictionary<string, object> dataMap, string description, Guid? jobId = null)
        {
            // Use DefaultGroup if jobGroup is null or empty
            jobGroup = (!string.IsNullOrEmpty(jobGroup)) ? jobGroup : JobKey.DefaultGroup;

            var jobbuilder = JobBuilder.Create(jobType);
            IJobDetail jobDetail = jobbuilder.WithDescription(description)
                .WithIdentity(jobName, jobGroup).StoreDurably(true).RequestRecovery(false)
                .Build();

            foreach (var mapItem in dataMap)
            {
                jobDetail.JobDataMap.Add(mapItem.Key, mapItem.Value);
            }

            Guid id;
            using (var tran = new TransactionScope())
            {
                id = _persistanceStore.UpsertJobKeyIdMap(jobName, jobGroup, jobId);
                _scheduler.AddJob(jobDetail, true);
                tran.Complete();
            }

            return id;
        }

        /// <summary>
        /// Deletes the specified job and all the asociated triggers.
        /// </summary>
        /// <param name="jobId"></param>
        public void RemoveJob(Guid jobId)
        {
            var jobKey = _persistanceStore.GetJobKey(jobId);

            if (_scheduler.CheckExists(jobKey))
            {
                _scheduler.DeleteJob(jobKey);

                // remove job key id map entry
                _persistanceStore.RemoveJobKeyIdMap(jobKey.Name, jobKey.Group);
            }
            else
            {
                throw new KeyNotFoundException(string.Format("JobKey not found for {0}, {1}", jobKey.Name, jobKey.Group));
            }
        }

        /// <summary>
        /// Remove trigger from scheduler.
        /// </summary>
        /// <param name="triggerId"></param>
        public void RemoveTrigger(Guid triggerId)
        {
            var triggerKey = _persistanceStore.GetTriggerKey(triggerId);

            if (_scheduler.CheckExists(triggerKey))
            {
                _scheduler.UnscheduleJob(triggerKey);

                // remove trigger key id map entry
                _persistanceStore.RemoveTriggerKeyIdMap(triggerKey.Name, triggerKey.Group);
            }
            else
            {
                throw new KeyNotFoundException(string.Format("TriggerKey not found for {0}", triggerId));
            }
        }

        /// <summary>
        /// Schedule specified trigger
        /// </summary>
        /// <param name="myTrigger"></param>
        public Guid ScheduleTrigger(BaseTrigger myTrigger)
        {
            Guid triggerId = Guid.Empty;

            // Set default values
            DateTimeOffset startAt = (DateTime.MinValue != myTrigger.StartDateTime) ? myTrigger.StartDateTime : DateTime.Now;

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

            var jobDataMap = new JobDataMap();
            if (myTrigger.JobDataMap != null && myTrigger.JobDataMap.Count > 0)
            {
                foreach (var jobData in myTrigger.JobDataMap)
                {
                    jobDataMap.Add(jobData.Key, jobData.Value);
                }
            }

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
                    .UsingJobData(jobDataMap)
                    .ModifiedByCalendar(!string.IsNullOrEmpty(cronTrigger.CalendarName) ? cronTrigger.CalendarName : null)
                    .WithCronSchedule(cronTrigger.CronExpression, misFireAction)
                    .StartAt(startAt)
                    .Build();

                trigger.TimeZone = TimeZoneInfo.Local;

                using (var tran = new TransactionScope())
                {
                    triggerId = _persistanceStore.UpsertTriggerKeyIdMap(myTrigger.Name, myTrigger.Group);
                    _scheduler.ScheduleJob(trigger);
                    tran.Complete();
                }
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
                    .UsingJobData(jobDataMap)
                    .ModifiedByCalendar(!string.IsNullOrEmpty(simpleTrigger.CalendarName) ? simpleTrigger.CalendarName : null)
                    .StartAt(startAt)
                    .WithSimpleSchedule(misFireAction)
                    .Build();

                using (var tran = new TransactionScope())
                {
                    triggerId = _persistanceStore.UpsertTriggerKeyIdMap(myTrigger.Name, myTrigger.Group);
                    _scheduler.ScheduleJob(trigger);
                    tran.Complete();
                }
            }

            return triggerId;
        }

        /// <summary>
        /// Get all triggers of a specified job
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IDictionary<ITrigger, Guid> GetTriggersOfJob(Guid id)
        {
            IDictionary<ITrigger, Guid> triggers = new Dictionary<ITrigger, Guid>();

            var jobKey = _persistanceStore.GetJobKey(id);

            var jobTriggers = _scheduler.GetTriggersOfJob(jobKey);

            foreach (var jobTrigger in jobTriggers)
            {
                var triggerId = _persistanceStore.GetTriggerId(jobTrigger.Key);

                triggers.Add(jobTrigger, triggerId);
            }

            return triggers;
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
        public Guid AddHolidayCalendar(string name, string description, IList<DateTime> daysExcludedUtc = null)
        {
            var holidays = new HolidayCalendar {Description = description};

            if (null != daysExcludedUtc && daysExcludedUtc.Count > 0)
            {
                foreach (var dateTime in daysExcludedUtc)
                {
                    holidays.AddExcludedDate(dateTime);
                }
            }

            Guid id;
            using (var tran = new TransactionScope())
            {
                id = _persistanceStore.UpsertCalendarIdMap(name);
                _scheduler.AddCalendar(name, holidays, true, true);
                tran.Complete();
            }

            return id;
        }

        /// <summary>
        /// Add exclusion dates to <see cref="HolidayCalendar"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="daysExcludedUtc"></param>
        public void AddHolidayCalendarExclusionDates(Guid id, IList<DateTime> daysExcludedUtc)
        {
            var name = _persistanceStore.GetCalendarName(id);

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
        public Guid AddCronCalendar(string name, string description, string cronExpression)
        {
            var cronCal = new CronCalendar(cronExpression) {Description = description};

            Guid id;
            using (var tran = new TransactionScope())
            {
                id = _persistanceStore.UpsertCalendarIdMap(name);
                _scheduler.AddCalendar(name, cronCal, true, true);
                tran.Complete();
            }

            return id;
        }

        /// <summary>
        /// Amends existing <see cref="CronCalendar"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="cronExpression"></param>
        public void AmendCronCalendar(Guid id, string description, string cronExpression)
        {
            var name = _persistanceStore.GetCalendarName(id);

            var cronCal = new CronCalendar(cronExpression) { Description = description };

            _scheduler.AddCalendar(name, cronCal, true, true);
        }

        /// <summary>
        /// Amends existing <see cref="HolidayCalendar"/>.
        /// New datesExcluded set replaces the current set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="datesExcluded"></param>
        public void AmendHolidayCalendar(Guid id, string description, IList<DateTime> datesExcluded)
        {
            var name = _persistanceStore.GetCalendarName(id);

            var holidays = (HolidayCalendar)_scheduler.GetCalendar(name);
            holidays.Description = description;

            // Remove currently excluded dates
            foreach (var excludedDate in holidays.ExcludedDates)
            {
                holidays.RemoveExcludedDate(excludedDate);
            }

            if (null != datesExcluded && datesExcluded.Count > 0)
            {
                foreach (var dateTime in datesExcluded)
                {
                    holidays.AddExcludedDate(dateTime);
                }
            }

            _scheduler.AddCalendar(name, holidays, true, true);
        }

        /// <summary>
        /// Delete Calendar
        /// </summary>
        /// <param name="id"></param>
        public bool DeleteCalendar(Guid id)
        {
            var name = _persistanceStore.GetCalendarName(id);

            bool retval = _scheduler.DeleteCalendar(name);

            // remove job key id map entry
            _persistanceStore.RemoveCalendarIdMap(name);

            return retval;
        }

        /// <summary>
        /// Get Calendar
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public ICalendar GetCalendar(Guid id, out string name)
        {
            name = _persistanceStore.GetCalendarName(id);

            return _scheduler.GetCalendar(name);
        }

        /// <summary>
        /// Get dictionary of ICalendar and (calendar) Name/Id pairs
        /// </summary>
        /// <returns></returns>
        public IDictionary<ICalendar, KeyValuePair<string, Guid>> GetCalendars()
        {
            var retval = new Dictionary<ICalendar, KeyValuePair<string, Guid>>();

            var calNames =  _scheduler.GetCalendarNames();

            foreach (var calName in calNames)
            {
                var quartzCal = _scheduler.GetCalendar(calName);
                var calId = _persistanceStore.GetCalendarId(calName);

                retval.Add(quartzCal, new KeyValuePair<string, Guid>(calName, calId));
            }

            return retval;
        }
    }
}
