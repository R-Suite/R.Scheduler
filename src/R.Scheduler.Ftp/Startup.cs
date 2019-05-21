using System;
using System.Linq;
using System.Reflection;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Ftp
{
    public class Startup : IJobTypeStartup
    {
        public void Initialise(IConfiguration config)
        {
            // Create Temp library,

            // Determine store type from configuration
            IFtpLibrary ftpLib = new FtpLibrary();

            if (!string.IsNullOrEmpty(config.CustomFtpLibraryAssemblyName))
            {
                var assemblyFileName = config.CustomFtpLibraryAssemblyName;

                if (!assemblyFileName.ToLower().EndsWith(".dll"))
                {
                    assemblyFileName += ".dll";
                }

                Assembly asm = Assembly.LoadFrom(assemblyFileName);

                Type pluginType = asm != null ? asm.GetTypes().Where(IsFtpLib).FirstOrDefault() : null;

                if (pluginType != null)
                {
                    ftpLib = (IFtpLibrary)Activator.CreateInstance(pluginType);
                }
            }
            
            SchedulerContainer.Container.Configure(c => c.AddRegistry<SmRegistry>());
            // Inject the persistence store after initialization
            SchedulerContainer.Container.Inject(ftpLib);
        }

        private static bool IsFtpLib(Type t)
        {
            if (t == null)
                return false;
            var isPlugin = t.GetInterface(typeof(IFtpLibrary).FullName) != null;
            if (!isPlugin)
                isPlugin = IsFtpLib(t.BaseType);
            return isPlugin;
        }
    }
}
