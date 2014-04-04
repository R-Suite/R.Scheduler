namespace R.Scheduler.Contracts.Interfaces
{
    public interface IPluginRepository
    {
        RegisteredPlugin GetRegisteredPlugin(string pluginName);
        void RegisterPlugin(RegisteredPlugin plugin);
    }
}
