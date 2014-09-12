using System;
using System.Collections.Generic;

namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecuteJob(Type jobType, Dictionary<string, object> dataMap);

        void RegisterPlugin(string pluginName, string assemblyPath);

        void RemovePlugin(string pluginName);

        void RemoveJobGroup(string groupName);

        void RemoveJob(string jobName, string jobGroup = null);

        void RemoveTriggerGroup(string groupName);

        void RemoveTrigger(string triggerName, string groupName = null);

        void ScheduleTrigger(BaseTrigger myTrigger, Type jobType);
    }
}
