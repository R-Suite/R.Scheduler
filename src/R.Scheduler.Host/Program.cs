using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace R.Scheduler.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialise scheduler
            Scheduler.Initialize(config =>
            {
                //config.PersistanceStoreType = PersistanceStoreType.Postgre;
                config.ConnectionString = "Server=localhost;Port=5432;Database=Scheduler;User Id=postgres;Password=schoolsoft;";
            });

            IScheduler sched = Scheduler.Instance();
            sched.Start();
        }
    }
}
