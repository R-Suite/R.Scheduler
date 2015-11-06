using Quartz;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap.Configuration.DSL;

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
            For<IScheduler>().Use(Scheduler.Instance);
            For<IAnalytics>().Use<Analytics>();
            // Default that my be overriden (using Sm TypeInterceptor) to use 
            // data store implementation selected in Scheduler Configuration
            For<IPersistanceStore>().Use<InMemoryStore>();
        }
    }
}
