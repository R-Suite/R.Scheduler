using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.PipesAndFilters
{
    internal sealed class FiltersAssemblyLoader : IFiltersAssemblyLoader
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _configLocation;

        public IFilter<T> LoadFilterFromPath<T>(string filterAssembleyPath, string jobDefinitionPath)
        {
            _configLocation = jobDefinitionPath;

            Logger.Info(string.Format("AppDomain.CurrentDomain.ShadowCopyFiles = {0}", AppDomain.CurrentDomain.ShadowCopyFiles));

            AppDomain.CurrentDomain.AssemblyResolve += LoadAssembly;

            var asm = Assembly.LoadFrom(filterAssembleyPath);

            string filterTypeName = GetFilterTypeName(filterAssembleyPath);

            Type filterType = null;
            try
            {
                filterType = asm.GetType(filterTypeName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting filter type.", ex);
            }

            var constructor = filterType.GetConstructors().FirstOrDefault(p => p.GetParameters().Length == 1 && p.GetParameters().First().ParameterType == typeof(string));

            if (constructor != null)
            {
                var filterParam = (IFilter<T>)Activator.CreateInstance(filterType, jobDefinitionPath);
                return filterParam;
            }

            var filterNoParam = (IFilter<T>)Activator.CreateInstance(filterType);

            return filterNoParam;
        }

        private Assembly LoadAssembly(object sender, ResolveEventArgs args)
        {
            Logger.Info("Executing LoadAssembly()");

            string folderPath = Path.GetDirectoryName(_configLocation);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");

            Logger.Info("Loading assembly - " + assemblyPath);

            if (File.Exists(assemblyPath) == false) return null;

            return Assembly.LoadFrom(assemblyPath);
        }

        private static string GetFilterTypeName(string pluginAssemblyPath)
        {
            string ret = null;
            if (!string.IsNullOrEmpty(pluginAssemblyPath) &&
                File.Exists(pluginAssemblyPath))
            {
                var asm = Assembly.LoadFrom(pluginAssemblyPath);
                var pluginType = asm != null ? asm.GetTypes().Where(IsFilter).FirstOrDefault() : null;
                ret = pluginType != null && !string.IsNullOrEmpty(pluginType.FullName) ? pluginType.FullName : null;
            }
            return ret;
        }

        private static bool IsFilter(Type t)
        {
            if (t == null)
                return false;
            var isFilter = t.GetInterface(typeof(IFilter<>).FullName) != null;
            if (!isFilter)
                isFilter = IsFilter(t.BaseType);
            return isFilter;
        }
    }
}
