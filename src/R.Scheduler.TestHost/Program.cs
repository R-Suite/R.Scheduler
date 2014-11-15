using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            R.Scheduler.Scheduler.Initialize(config =>
            {
                config.EnableWebApiSelfHost = true;
                config.PersistanceStoreType = PersistanceStoreType.InMemory;
            });

            IScheduler sched = R.Scheduler.Scheduler.Instance();
            sched.Start();
        }
    }
}
