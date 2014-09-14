using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecuteJob(Type jobType, Dictionary<string, object> dataMap);

        void RemoveJobGroup(string groupName);

        void RemoveJob(string jobName, string jobGroup = null);

        void RemoveTriggerGroup(string groupName);

        void RemoveTrigger(string triggerName, string groupName = null);

        void ScheduleTrigger(BaseTrigger myTrigger, Type jobType);

        IList<ITrigger> GetTriggersOfJobGroup(string groupName);
    }
}
