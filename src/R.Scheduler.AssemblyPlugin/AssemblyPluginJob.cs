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
            Logger.Debug("Entering PluginRunner.ctor().");
        }

        /// <summary>
        /// Entry point into the job execution.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var jobName = context.JobDetail.Key.Name;

            string pluginPath = dataMap.GetString("pluginPath");

            Logger.DebugFormat("Entering PluginRunner.Execute(). pluginPath = {0}", pluginPath);

            if (string.IsNullOrEmpty(pluginPath) || !File.Exists(pluginPath))
            {
                Logger.ErrorFormat("Error in AssemblyPluginJob ({0}): plugin file '{1}' does not exist.", jobName, pluginPath);
                throw new JobExecutionException(string.Format("Assembly file {0} does not exist", pluginPath));
            }

            var pluginAssemblyName = Path.GetFileNameWithoutExtension(pluginPath);

            var appDomain = GetAppDomain(pluginPath, pluginAssemblyName);
            var pluginTypeName = GetPluginTypeName(appDomain, pluginPath, jobName);
            Exception executionException = null;

            IJobPlugin jobPlugin;
            try
            {
                jobPlugin = appDomain.CreateInstanceAndUnwrap(pluginAssemblyName, pluginTypeName) as IJobPlugin;
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("Error creating instance of IJobPlugin. pluginTypeName = {0}.", pluginTypeName), ex);
                jobPlugin = null;
                executionException = ex;
            }

            try
            {
                if (jobPlugin != null)
                {
                    jobPlugin.Execute();
                    Logger.DebugFormat("Job Executed. pluginPath = {0}", pluginPath);
                }
                else
                {
                    executionException = new Exception(string.Format("AssemblyPlugin cannot be null {0}.", pluginTypeName));
                }
            }
            catch (Exception ex)
            {
                executionException = ex;
            }

            AppDomain.Unload(appDomain);

            if (null != executionException)
            {
                Logger.Error(string.Format("Error in AssemblyPluginJob ({0}):", jobName), executionException);
                throw new JobExecutionException(executionException.Message, executionException, false);
            }
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

        private static string GetPluginTypeName(AppDomain domain, string pluginPath, string jobName)
        {
            PluginAppDomainHelper helper = null;
            var pluginFinderType = typeof (PluginAppDomainHelper);

            if (!string.IsNullOrEmpty(pluginFinderType.FullName))
                helper =
                    domain.CreateInstanceAndUnwrap(pluginFinderType.Assembly.FullName, pluginFinderType.FullName, false,
                        BindingFlags.CreateInstance, null, new object[] {pluginPath}, null, null) as
                        PluginAppDomainHelper;

            if (helper == null)
            {
                Logger.ErrorFormat("Error in AssemblyPluginJob ({0}): Could not create plugin domain helper.", jobName);
                throw new JobExecutionException("Could not create plugin domain helper");
            }

            return helper.PluginTypeName;
        }

        #endregion
    }
}
