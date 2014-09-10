namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecutePlugin(string pluginName);

        void RegisterPlugin(string pluginName, string assemblyPath);

        void RemovePlugin(string pluginName);

        void DescheduleJobGroup(string groupName);

        void DescheduleTrigger(string groupName, string triggerName);

        void ScheduleTrigger(BaseTrigger myTrigger);
    }
}
