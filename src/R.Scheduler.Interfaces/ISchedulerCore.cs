namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecutePlugin(string pluginName);

        void RegisterPlugin(string pluginName, string assemblyPath);

        void RemovePlugin(string pluginName);

        void DescheduleGroup(string groupName);

        void DescheduleTrigger(string groupName, string triggerName);

        void ScheduleSimpleTrigger(SimpleTrigger simpleTrigger);

        void ScheduleCronTrigger(CronTrigger cronTrigger);
    }
}
