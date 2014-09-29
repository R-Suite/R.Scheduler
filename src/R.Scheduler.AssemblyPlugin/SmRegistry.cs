using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.AssemblyPlugin.Persistance;
using R.Scheduler.Interfaces;
using StructureMap.Configuration.DSL;

namespace R.Scheduler.AssemblyPlugin
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IPluginStore>().Use<InMemoryPluginStore>();// default that my be overriden (using inteceptor) to use data store selected in Scheduler Configuration
            For<IJobTypeStartup>().Use<Startup>();
            For<IJobTypeManager>().Use<PluginManager>();
        }
    }
}
