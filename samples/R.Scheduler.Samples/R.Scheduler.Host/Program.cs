using System;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("R.Scheduler is host at {0}", DateTime.UtcNow);

            Scheduler.Initialize(config =>
            {
                config.PersistanceStoreType = PersistanceStoreType.InMemory;
                config.EnableAuditHistory = false;
            });

            IScheduler sched = R.Scheduler.Scheduler.Instance();
            sched.Start();
        }
    }
}
