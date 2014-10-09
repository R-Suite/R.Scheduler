using System;
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

        Plugin GetRegisteredPlugin(Guid id);

        IList<Plugin> GetRegisteredPlugins();

        void RegisterPlugin(Plugin plugin);

        void UpdatePluginName(Guid id, string name);

        int RemovePlugin(string pluginName);

        int RemoveAllPlugins();

        PluginDetails GetRegisteredPluginDetails(string pluginName);
    }
}
