using System;
using System.Linq.Expressions;
using Quartz;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler
{
    /// <summary>
    /// StructureMap registry
    /// </summary>
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<ISchedulerCore>().Use<SchedulerCore>();
            For<IAnalytics>().Use<Analytics>();
            For<IPermissionsHelper>().Use<PermissionsHelper>();
            // Default that my be overriden (using Sm TypeInterceptor) to use 
            // data store implementation selected in Scheduler Configuration
        }
    }
}
