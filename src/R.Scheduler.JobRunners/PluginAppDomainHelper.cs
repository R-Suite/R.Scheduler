using System;
using System.IO;
using System.Linq;
using System.Reflection;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.JobRunners
{
    /// <summary>
    /// PluginAppDomainHelper provides help methods for resolving assemblies and reflecting on types 
    /// within an AppDomain used for JobPlugin execution.
    /// </summary>
    [Serializable]
    public class PluginAppDomainHelper : MarshalByRefObject
    {
        private readonly string _pluginAssemblyPath;

        public PluginAppDomainHelper(string pluginAssemblyPath)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            _pluginAssemblyPath = pluginAssemblyPath;
        }

        /// <summary>
        /// Fullname of IJobPlugin type
        /// </summary>
        public string PluginTypeName
        {
            get
            {
                string ret = null;
                if (!string.IsNullOrEmpty(_pluginAssemblyPath) && File.Exists(_pluginAssemblyPath))
                {
                    var asm = Assembly.LoadFrom(_pluginAssemblyPath);
                    var pluginType = asm != null ? asm.GetTypes().Where(IsPlugin).FirstOrDefault() : null;
                    ret = pluginType != null && !string.IsNullOrEmpty(pluginType.FullName) ? pluginType.FullName : null;
                }
                return ret;
            }
        }

        /// <summary>
        /// Verify the plugin assembly contains a valid plugin type
        /// </summary>
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
            var assemblyFolder = Path.GetDirectoryName(_pluginAssemblyPath);

            if (string.IsNullOrEmpty(assemblyFolder) || !Directory.Exists(assemblyFolder))
                return null;

            var name = args.Name.Split(new[] { ',' })[0];
            var assemblyPath = Path.Combine(assemblyFolder, name + ".dll");

            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }
    }
}
