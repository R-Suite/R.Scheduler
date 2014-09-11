using Quartz;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap.Configuration.DSL;

namespace R.Scheduler
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IPluginStore>().Use<PostgrePluginStore>();
            For<ISchedulerCore>().Use<SchedulerCore>();
            For<IScheduler>().Use(Scheduler.Instance);
        }
    }
}
