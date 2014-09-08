using System.Collections.Generic;
using R.Scheduler.Contracts.DataContracts;

namespace R.Scheduler.Interfaces
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
