using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Ftp
{
    public class Startup : IJobTypeStartup
    {
        public void Initialise(IConfiguration config)
        {
            ObjectFactory.Configure(x => x.RegisterInterceptor(new FtpLibraryInterceptor(config)));
        }
    }
}
