using R.Scheduler.Interfaces;
using StructureMap.Configuration.DSL;

namespace R.Scheduler.Ftp
{
    public class SmRegistry : Registry
    {
        public SmRegistry()
        {
            For<IJobTypeStartup>().Use<Startup>();
            For<IFtpLibrary>().Use<FtpLibrary>();
        }
    }
}
