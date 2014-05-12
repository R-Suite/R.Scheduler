using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;
using StructureMap;

namespace R.Scheduler.JobRunners
{
    public class PluginRunner : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IBus Bus { get; set; }

        public PluginRunner()
        {
            Bus = ObjectFactory.Container.GetInstance<IBus>();
        }

        public PluginRunner(IBus bus)
        {
            Bus = bus;
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string pluginPath = dataMap.GetString("pluginPath");

            if (string.IsNullOrEmpty(pluginPath) || !File.Exists(pluginPath))
            {
                Logger.Error(string.Format("plugin file '{0}' does not exist.", pluginPath));
                return;
            }

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
                ApplicationBase = appBase,
                PrivateBinPath = privateBinPath,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = assemblyFolderPath,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };

            var assemblyName = Path.GetFileNameWithoutExtension(pluginPath);
            var domain = AppDomain.CreateDomain(Guid.NewGuid() + "_" + assemblyName, null, setup);

            // Load PluginAppDomainHelper into new AppDomain to get plugin type using reflection
            PluginAppDomainHelper helper = null;
            var pluginFinderType = typeof(PluginAppDomainHelper);
            if (!string.IsNullOrEmpty(pluginFinderType.FullName))
                helper = domain.CreateInstanceAndUnwrap(pluginFinderType.Assembly.FullName, pluginFinderType.FullName) as PluginAppDomainHelper;
            if (helper == null)
                throw new Exception("Couldn't create plugin domain helper");
            helper.PluginAssemblyPath = pluginPath;
            
            var pluginTypeName = helper.PluginTypeName;
            var jobPlugin = domain.CreateInstanceAndUnwrap(assemblyName, pluginTypeName) as IJobPlugin;

            bool success = false;
            try
            {
                if (jobPlugin != null)
                {
                    jobPlugin.Execute();
                    success = true;
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

            Bus.Publish(new JobExecutedMessage(Guid.NewGuid()) { Success = success, Timestamp = DateTime.UtcNow, Type = pluginTypeName });

            helper = null;
            AppDomain.Unload(domain);
        }
    }
}
