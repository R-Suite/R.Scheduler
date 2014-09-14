using System.Collections.Generic;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;

namespace R.Scheduler.AssemblyPlugin.Interfaces
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

        int RemovePlugin(string pluginName);

        int RemoveAllPlugins();

        PluginDetails GetRegisteredPluginDetails(string pluginName);
    }
}
