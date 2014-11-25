using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap.Configuration.DSL;

namespace R.Scheduler.PipesAndFilters
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IPersistanceStore>().Use<InMemoryStore>();// default that my be overriden (using inteceptor) to use data store selected in Scheduler Configuration
            For<IJobTypeStartup>().Use<Startup>();
        }
    }
}
