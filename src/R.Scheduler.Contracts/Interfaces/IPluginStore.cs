namespace R.Scheduler.Contracts.Interfaces
{
    public enum PluginStoreType
    {
        InMemory,
        Postgre
    }

    public interface IPluginStore
    {
        Plugin GetRegisteredPlugin(string pluginName);

        void RegisterPlugin(Plugin plugin);
    }
}
