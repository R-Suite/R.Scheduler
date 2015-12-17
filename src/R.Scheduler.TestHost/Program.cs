using R.Scheduler.Interfaces;

namespace R.Scheduler.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //R.Scheduler.Scheduler.Initialize(config =>
            //{
            //    config.EnableWebApiSelfHost = true;
            //    config.PersistanceStoreType = PersistanceStoreType.Postgre;
            //    config.ConnectionString = "Server=127.0.0.1;Port=5432;Database=Scheduler;User Id=postgres;Password=postgres;";
            //    //config.CustomFtpLibraryAssemblyName = "R.Scheduler.FakeFtpLib";
            //});
            R.Scheduler.Scheduler.Initialize(config =>
            {
                config.EnableWebApiSelfHost = true;
                config.PersistanceStoreType = PersistanceStoreType.Postgre;
                config.ConnectionString = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Scheduler;";
                //config.CustomFtpLibraryAssemblyName = "R.Scheduler.FakeFtpLib";
            });
        }
    }
}
