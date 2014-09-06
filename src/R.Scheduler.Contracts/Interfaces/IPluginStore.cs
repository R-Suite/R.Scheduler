using System.Collections.Generic;

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

        IList<Plugin> GetRegisteredPlugins();

        void RegisterPlugin(Plugin plugin);

        PluginDetails GetRegisteredPluginDetails(string pluginName);
    }
}
