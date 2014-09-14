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
            For<IPluginStore>().Use<PostgrePluginStore>();
            For<IJobTypeManager>().Use<PluginManager>();
        }
    }
}
