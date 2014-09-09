namespace R.Scheduler.Interfaces
{
    public interface ISchedulerCore
    {
        void ExecutePlugin(string pluginName);

        void RegisterPlugin(string pluginName, string assemblyPath);

        void RemovePlugin(string pluginName);

        void DeschedulePlugin(string pluginName);
    }
}
