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
                config.PersistanceStoreType = PersistanceStoreType.SqlServer;
                //config.ConnectionString = @"Server=RUFFER-716Q85J\MSSQLSERVER2012; DataBase=RufferScheduler;Trusted_Connection=True;";
                config.ConnectionString = @"Server=lonsqltst02; DataBase=RufferScheduler;Trusted_Connection=True;";
            });

            IScheduler sched = R.Scheduler.Scheduler.Instance();
            sched.Start();
        }
    }
}
