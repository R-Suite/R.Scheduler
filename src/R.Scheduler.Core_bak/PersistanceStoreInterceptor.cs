using System;
using System.Linq;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;
using StructureMap.Interceptors;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Everytime a default instance of <see cref="IPersistanceStore"/> is created by StructureMap,
    /// replace it with an implementation configured during the scheduler startup.
    /// Used mainly for injecting the IPersistanceStore implementation 
    /// into the StructureMap registries of CustomJobTypes projects to ensure 
    /// a single persistence store for monitoring and auditing different types of jobs. 
    /// </summary>
    public class PersistanceStoreInterceptor : TypeInterceptor
    {
        private readonly IConfiguration _config;

        public PersistanceStoreInterceptor(IConfiguration config)
        {
            _config = config;
        }

        public object Process(object target, IContext context)
        {
            IPersistanceStore retval = null;

            if (target.GetType().GetInterfaces().Contains(typeof(IPersistanceStore)))
            {
                switch (_config.PersistanceStoreType)
                {
                    case PersistanceStoreType.Postgre:
                        retval = new PostgreStore(_config.ConnectionString);
                        break;
                    case PersistanceStoreType.SqlServer:
                        retval = new SqlServerStore(_config.ConnectionString);
                        break;
                    case PersistanceStoreType.InMemory:
                        retval = new InMemoryStore();
                        break;
                }
            }

            return retval;
        }

        public bool MatchesType(Type type)
        {
            if (type.GetInterfaces().Contains(typeof(IPersistanceStore)))
            {
                return true;
            }

            return false;
        }
    }
}
