using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.PipesAndFilters
{
    public class Startup : IJobTypeStartup
    {
        public void Initialise(IConfiguration config)
        {
            ObjectFactory.Configure(x => x.RegisterInterceptor(new PersistanceStoreInterceptor(config)));
        }
    }
}
