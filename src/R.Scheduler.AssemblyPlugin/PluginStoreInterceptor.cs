using System;
using System.Linq;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.AssemblyPlugin.Persistance;
using R.Scheduler.Interfaces;
using StructureMap;
using StructureMap.Interceptors;

namespace R.Scheduler.AssemblyPlugin
{
    class PluginStoreInterceptor : TypeInterceptor
    {
        private readonly IConfiguration _config;

        public PluginStoreInterceptor(IConfiguration config)
        {
            _config = config;
        }

        public object Process(object target, IContext context)
        {
            IPluginStore retval = null;

            if (target.GetType().GetInterfaces().Contains(typeof(IPluginStore)))
            {
                switch (_config.PersistanceStoreType)
                {
                    case PersistanceStoreType.Postgre:
                        retval = new PostgrePluginStore();
                        break;
                    case PersistanceStoreType.SqlServer:
                        retval = new SqlServerPluginStore();
                        break;
                    case PersistanceStoreType.InMemory:
                        retval = new InMemoryPluginStore();
                        break;
                }
            }

            return retval;
        }

        public bool MatchesType(Type type)
        {
            if (type.GetInterfaces().Contains(typeof(IPluginStore)))
            {
                return true;
            }

            return false;
        }
    }
}
