using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        string SchedulerName { get; }

        IDictionary<IJobDetail, Guid> GetJobDetails(Type jobType = null);

        IJobDetail GetJobDetail(Guid id);

        void ExecuteJob(Guid jobId);

        Guid CreateJob(string jobName, string groupName, Type jobType, Dictionary<string, object> dataMap, string description, Guid? jobId = null);

        void RemoveJobTriggers(Guid jobId);

        void RemoveJob(Guid jobId);

        void RemoveTrigger(Guid triggerId);

        void PauseTrigger(Guid triggerId);

        void ResumeTrigger(Guid triggerId);

        Guid ScheduleTrigger(BaseTrigger myTrigger);

        IDictionary<ITrigger, Guid> GetTriggersOfJob(Guid id);

        IEnumerable<TriggerFireTime> GetFireTimesBetween(DateTime start, DateTime end);

        Guid AddHolidayCalendar(string name, string description, IList<DateTime> daysExcludedUtc = null);

        void AddHolidayCalendarExclusionDates(Guid id, IList<DateTime> daysExcludedUtc);

        Guid AddCronCalendar(string name, string description, string cronExpression);

        void AmendCronCalendar(Guid id, string description, string cronExpression);

        void AmendHolidayCalendar(Guid id, string description, IList<DateTime> datesExcluded);

        bool DeleteCalendar(Guid id);

        ICalendar GetCalendar(Guid id, out string name);

        IDictionary<ICalendar, KeyValuePair<string, Guid>> GetCalendars();
    }
}
