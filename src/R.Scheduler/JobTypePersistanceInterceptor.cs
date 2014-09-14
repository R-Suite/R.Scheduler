using System;
using System.Linq;
using R.Scheduler.Interfaces;
using StructureMap;
using StructureMap.Interceptors;

namespace R.Scheduler
{
    /// <summary>
    /// Injects ConnectioString into all the [data access] classes implementing IUseSchedulerConnectionString
    /// </summary>
    internal class JobTypePersistanceInterceptor : TypeInterceptor
    {
        private readonly string _connectionString;

        public JobTypePersistanceInterceptor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public object Process(object target, IContext context)
        {
            if (target.GetType().GetInterfaces().Contains(typeof(IUseSchedulerConnectionString)))
            {
                ((IUseSchedulerConnectionString)target).SetConnectionString(_connectionString);
            }

            return target;
        }

        public bool MatchesType(Type type)
        {
            if (type.GetInterfaces().Contains(typeof(IUseSchedulerConnectionString)))
            {
                return true;
            }

            return false;
        }
    }
}