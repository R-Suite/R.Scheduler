namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecutePlugin(string pluginName);

        void RegisterPlugin(string pluginName, string assemblyPath);

        void RemovePlugin(string pluginName);

        void RemoveJobGroup(string groupName);

        void RemoveJob(string jobName, string jobGroup = null);

        void RemoveTriggerGroup(string groupName);

        void RemoveTrigger(string triggerName, string groupName = null);

        void ScheduleTrigger(BaseTrigger myTrigger);
    }
}
