using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Persistance.Postgre;
using StructureMap.Configuration.DSL;

namespace R.Scheduler
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IPluginStore>().Use<PluginStore>();
        }
    }
}
