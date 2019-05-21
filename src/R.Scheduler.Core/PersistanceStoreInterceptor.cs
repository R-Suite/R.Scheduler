using System;
using System.Collections.Generic;
using System.Linq;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistence;
using StructureMap.Building.Interception;
using StructureMap.Pipeline;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Every time a default instance of <see cref="IPersistenceStore"/> is created by StructureMap,
    /// replace it with an implementation configured during the scheduler startup.
    /// Used mainly for injecting the IPersistenceStore implementation 
    /// into the StructureMap registries of CustomJobTypes projects to ensure 
    /// a single persistence store for monitoring and auditing different types of jobs. 
    /// </summary>
    public class PersistanceStoreInterceptor : IInterceptorPolicy
    {
        private readonly IConfiguration _config;

        public PersistanceStoreInterceptor(IConfiguration config)
        {
            _config = config;
        }

        public string Description { get; }

        public IEnumerable<IInterceptor> DetermineInterceptors(Type pluginType, Instance instance)
        {
            if (pluginType.GetInterfaces().Contains(typeof(IPersistenceStore)))
            {
                yield return new ActivatorInterceptor<IPersistenceStore>((context, x) => this.Activate(x));
            }
        }

        private IPersistenceStore Activate(object target)
        {
            IPersistenceStore retval = null;

            if (target.GetType().GetInterfaces().Contains(typeof(IPersistenceStore)))
            {
                switch (_config.PersistenceStoreType)
                {
                    case PersistenceStoreType.Postgre:
                        retval = new PostgreStore(_config.ConnectionString);
                        break;
                    case PersistenceStoreType.SqlServer:
                        retval = new SqlServerStore(_config.ConnectionString);
                        break;
                    case PersistenceStoreType.InMemory:
                        retval = new InMemoryStore();
                        break;
                }
            }

            return retval;
        }
    }
}
