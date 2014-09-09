using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// InMemory implementation of IPluginStore
    /// </summary>
    public class InMemoryPluginStore : IPluginStore
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private CacheItemPolicy _policy; 

        /// <summary>
        /// Get registered plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public Plugin GetRegisteredPlugin(string pluginName)
        {
            Plugin retval = null;

            if (Cache.Contains(pluginName))
            {
                retval = Cache[pluginName] as Plugin; 
            }

            return retval;
        }

        /// <summary>
        /// Get all registered plugin
        /// </summary>
        /// <returns></returns>
        public IList<Plugin> GetRegisteredPlugins()
        {
            IList<Plugin> retval = new List<Plugin>();

            if (Cache.GetCount() > 0)
            {
                foreach (KeyValuePair<string, object> caheItem in Cache)
                {
                    retval.Add(new Plugin
                    {
                        Id = ((Plugin)caheItem.Value).Id,
                        Name = caheItem.Key,
                        AssemblyPath = ((Plugin)caheItem.Value).AssemblyPath,
                        Status = ((Plugin)caheItem.Value).Status
                    });
                }
            }

            return retval;
        }

        /// <summary>
        /// Register new plugin, or update existing one.
        /// </summary>
        /// <param name="plugin"></param>
        public void RegisterPlugin(Plugin plugin)
        {
            _policy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddHours(10.00)};

            if (!Cache.Contains(plugin.Name))
            {
                plugin.Id = Guid.NewGuid();
            }

            Cache.Set(plugin.Name, plugin, _policy); 
        }

        /// <summary>
        /// Remove plugin from cache object
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public int RemovePlugin(string pluginName)
        {
            if (Cache.Contains(pluginName))
            {
                Cache.Remove(pluginName);

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Remove all pugins from cache object
        /// </summary>
        /// <returns></returns>
        public int RemoveAllPlugins()
        {
            if (Cache.GetCount() > 0)
            {
                int count = 0;
                foreach (var element in Cache)
                {
                    Cache.Remove(element.Key);
                    count++;
                }

                return count;
            }

            return 0;
        }

        public PluginDetails GetRegisteredPluginDetails(string pluginName)
        {
            throw new NotImplementedException();
        }
    }
}
