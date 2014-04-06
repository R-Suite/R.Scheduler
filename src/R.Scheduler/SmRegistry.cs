using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Persistance;
using StructureMap.Configuration.DSL;

namespace R.Scheduler
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IPluginStore>().Use<PostgrePluginStore>();
        }
    }
}
