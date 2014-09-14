using Quartz;
using R.Scheduler.Interfaces;
using StructureMap.Configuration.DSL;

namespace R.Scheduler
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<ISchedulerCore>().Use<SchedulerCore>();
            For<IScheduler>().Use(Scheduler.Instance);
        }
    }
}
