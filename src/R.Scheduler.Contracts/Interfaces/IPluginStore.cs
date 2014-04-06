namespace R.Scheduler.Contracts.Interfaces
{
    public interface IPluginStore
    {
        Plugin GetRegisteredPlugin(string pluginName);

        void RegisterPlugin(Plugin plugin);
    }
}
