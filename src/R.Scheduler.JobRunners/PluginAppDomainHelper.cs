using System;
using System.IO;
using System.Linq;
using System.Reflection;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.JobRunners
{

    [Serializable]
    public class PluginAppDomainHelper : MarshalByRefObject
    {
        public PluginAppDomainHelper()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        public string PluginAssemblyPath
        {
            get;
            set;
        }

        public string PluginTypeName
        {
            get
            {
                string ret = null;
                if (!string.IsNullOrEmpty(PluginAssemblyPath) && File.Exists(PluginAssemblyPath))
                {
                    var asm = Assembly.LoadFrom(PluginAssemblyPath);
                    var pluginType = asm != null ? asm.GetTypes().Where(IsPlugin).FirstOrDefault() : null;
                    ret = pluginType != null && !string.IsNullOrEmpty(pluginType.FullName) ? pluginType.FullName : null;
                }
                return ret;
            }
        }

        public bool IsValidPlugin
        {
            get
            {
                return !string.IsNullOrEmpty(PluginTypeName);
            }
        }

        private static bool IsPlugin(Type t)
        {
            if (t == null)
                return false;
            var isPlugin = t.GetInterface(typeof(IJobPlugin).FullName) != null;
            if (!isPlugin)
                isPlugin = IsPlugin(t.BaseType);
            return isPlugin;
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyFolder = Path.GetDirectoryName(PluginAssemblyPath);

            if (string.IsNullOrEmpty(assemblyFolder) || !Directory.Exists(assemblyFolder))
                return null;

            var name = args.Name.Split(new[] { ',' })[0];
            var assemblyPath = Path.Combine(assemblyFolder, name + ".dll");

            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }
    }
}
