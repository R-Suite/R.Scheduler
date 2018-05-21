using System;
using System.Linq;
using System.Reflection;
using R.Scheduler.Interfaces;
using StructureMap;
using StructureMap.Interceptors;

namespace R.Scheduler.Ftp
{
    /// <summary>
    /// Everytime a default instance of <see cref="IFtpLibrary"/> is created by StructureMap,
    /// replace it with an implementation configured during the scheduler startup.
    /// </summary>
    public class FtpLibraryInterceptor : TypeInterceptor
    {
        private readonly IConfiguration _config;

        public FtpLibraryInterceptor(IConfiguration config)
        {
            _config = config;
        }

        public object Process(object target, IContext context)
        {
            IFtpLibrary retval = (IFtpLibrary) target;

            if (!string.IsNullOrEmpty(_config.CustomFtpLibraryAssemblyName) && target.GetType().GetInterfaces().Contains(typeof(IFtpLibrary)))
            {
                var assemblyFileName = _config.CustomFtpLibraryAssemblyName;

                if (!assemblyFileName.ToLower().EndsWith(".dll"))
                {
                    assemblyFileName += ".dll";
                }

                Assembly asm = Assembly.LoadFrom(assemblyFileName);

                Type pluginType = asm != null ? asm.GetTypes().Where(IsFtpLib).FirstOrDefault() : null;

                if (pluginType != null)
                {
                    retval = (IFtpLibrary) Activator.CreateInstance(pluginType);
                }
            }

            return retval;
        }

        public bool MatchesType(Type type)
        {
            if (type.GetInterfaces().Contains(typeof(IFtpLibrary)))
            {
                return true;
            }

            return false;
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
