using R.Scheduler.Contracts.Interfaces;
using StructureMap;

namespace R.Scheduler
{
    public class Configuration : IConfiguration
    {
        public void SetPluginStore<T>() where T : class, IPluginStore
        {
            ObjectFactory.Configure(x => x.For<IPluginStore>().Use<T>());
        }
    }
}
