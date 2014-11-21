using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        IEnumerable<IJobDetail> GetJobDetails(Type jobType = null);

        IJobDetail GetJobDetail(string jobName, string groupName = null);

        void ExecuteJob(string jobName, string groupName);

        void RemoveJobTriggers(string jobName, string groupName);


        void ExecuteJob(Type jobType, Dictionary<string, object> dataMap);

        void RemoveJob(string jobName, string jobGroup = null);

        void RemoveJobGroup(string groupName);

        void RemoveTriggerGroup(string groupName);

        void RemoveTrigger(string triggerName, string groupName = null);

        void ScheduleTrigger(BaseTrigger myTrigger, Type jobType);

        IEnumerable<ITrigger> GetTriggersOfJobGroup(string groupName);
    }
}
