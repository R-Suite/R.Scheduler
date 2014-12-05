using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        string SchedulerName { get; }

        IEnumerable<IJobDetail> GetJobDetails(Type jobType = null);

        IJobDetail GetJobDetail(string jobName, string jobGroup);

        void ExecuteJob(string jobName, string groupName);

        void CreateJob(string jobName, string groupName, Type jobType, Dictionary<string, object> dataMap);

        void RemoveJobTriggers(string jobName, string groupName);

        void RemoveJob(string jobName, string jobGroup);

        void RemoveTrigger(string triggerName, string groupName);

        void ScheduleTrigger(BaseTrigger myTrigger);

        IEnumerable<ITrigger> GetTriggersOfJob(string jobName, string jobGroup);

        void AddHolidayCalendar(string name, string description, IList<DateTime> daysExcludedUtc = null);

        void AddHolidayCalendarExclusionDates(string name, IList<DateTime> daysExcludedUtc);

        bool DeleteCalendar(string name);
    }
}
