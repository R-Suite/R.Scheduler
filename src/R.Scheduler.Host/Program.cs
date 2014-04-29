using System;
using System.ServiceProcess;
using Quartz;

namespace R.Scheduler.Host
{
    class Program
    {
        #region Nested classes to support running as service

        public const string ServiceName = "R.Scheduler.Host";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }

        #endregion

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);

                Console.WriteLine();
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        private static void Start(string[] args)
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

        private static void Stop()
        {
            IScheduler sched = Scheduler.Instance();
            sched.Shutdown();
        }
    }
}
