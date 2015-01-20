using System;
using System.IO;
using System.Reflection;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin;

namespace R.Scheduler.AssemblyPlugin
{
    /// <summary>
    /// PluginRunner loads and executes JobPlugins within a separate AppDomain.
    /// </summary>
    public class AssemblyPluginJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Ctor used by Scheduler engine
        /// </summary>
        public AssemblyPluginJob()
        {
            Logger.Info("Entering PluginRunner.ctor().");
        }

        /// <summary>
        /// Entry point into the job execution.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string pluginPath = dataMap.GetString("pluginPath");

            Logger.Info("Entering PluginRunner.Execute(). pluginPath=" + pluginPath);

            if (string.IsNullOrEmpty(pluginPath) || !File.Exists(pluginPath))
            {
                Logger.Error(string.Format("plugin file '{0}' does not exist.", pluginPath));
                return;
            }

            var pluginAssemblyName = Path.GetFileNameWithoutExtension(pluginPath);

            var appDomain = GetAppDomain(pluginPath, pluginAssemblyName);
            var pluginTypeName = GetPluginTypeName(appDomain, pluginPath);

            IJobPlugin jobPlugin;
            try
            {
                jobPlugin = appDomain.CreateInstanceAndUnwrap(pluginAssemblyName, pluginTypeName) as IJobPlugin;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error creating instance of IJobPlugin. {0}.", ex.Message));
                jobPlugin = null;
            }

            try
            {
                if (jobPlugin != null)
                {
                    jobPlugin.Execute();
                    Logger.Info("Job Executed. pluginPath=" + pluginPath);
                }
                else
                {
                    Logger.Error(string.Format("Plugin cannot be null {0}.", pluginTypeName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error occured in {0}.", pluginTypeName), ex);
            }

            AppDomain.Unload(appDomain);
        }

        #region Private Methods

        private static AppDomain GetAppDomain(string pluginPath, string pluginAssemblyName)
        {
            var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var assemblyFolderPath = Path.GetDirectoryName(pluginPath);
            var privateBinPath = assemblyFolderPath;

            if (assemblyFolderPath != null && assemblyFolderPath.StartsWith(appBase))
            {
                privateBinPath = assemblyFolderPath.Replace(appBase, string.Empty);
                if (privateBinPath.StartsWith(@"\"))
                    privateBinPath = privateBinPath.Substring(1);
            }

            var setup = new AppDomainSetup
            {
                ApplicationName = pluginAssemblyName,
                CachePath = assemblyFolderPath,
                ApplicationBase = appBase,
                PrivateBinPath = privateBinPath,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = assemblyFolderPath,
                ConfigurationFile = pluginPath + ".config",
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };

            var appDomain = AppDomain.CreateDomain(Guid.NewGuid() + "_" + pluginAssemblyName, null, setup);

            return appDomain;
        }

        private static string GetPluginTypeName(AppDomain domain, string pluginPath)
        {
            PluginAppDomainHelper helper = null;
            var pluginFinderType = typeof (PluginAppDomainHelper);

            if (!string.IsNullOrEmpty(pluginFinderType.FullName))
                helper =
                    domain.CreateInstanceAndUnwrap(pluginFinderType.Assembly.FullName, pluginFinderType.FullName, false,
                        BindingFlags.CreateInstance, null, new object[] {pluginPath}, null, null) as
                        PluginAppDomainHelper;

            if (helper == null)
                throw new Exception("Couldn't create plugin domain helper");

            return helper.PluginTypeName;
        }

        #endregion
    }
}
