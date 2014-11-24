using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        string SchedulerName { get; }

        IEnumerable<IJobDetail> GetJobDetails(Type jobType = null);

        void ExecuteJob(string jobName, string groupName);

        void CreateJob(string jobName, string groupName, Type jobType, Dictionary<string, object> dataMap);

        void RemoveJobTriggers(string jobName, string groupName);


        void ExecuteJob(Type jobType, Dictionary<string, object> dataMap);

        void RemoveJob(string jobName, string jobGroup = null);

        void RemoveJobGroup(string groupName);

        void RemoveTriggerGroup(string groupName);

        void RemoveTrigger(string triggerName, string groupName = null);

        void ScheduleTrigger(BaseTrigger myTrigger, Type jobType);

        void ScheduleTrigger(BaseTrigger myTrigger);

        IEnumerable<ITrigger> GetTriggersOfJobGroup(string jobGroup);

        IEnumerable<ITrigger> GetTriggersOfJob(string jobName, string jobGroup = null);
    }
}
