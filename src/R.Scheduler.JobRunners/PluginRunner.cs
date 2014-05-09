using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.JobRunners
{
    public class PluginRunner : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IProducer _producer;

        public PluginRunner()
        {
            
        }

        public PluginRunner(IProducer producer)
        {
            _producer = producer;
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

            //todo: load plugins into new app domain
            /*
            var setup = new AppDomainSetup
            {
                PrivateBinPath = privateBinPath,
                ApplicationBase = appBase,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = assemblyFolderPath
            };

            var assemblyName = Path.GetFileNameWithoutExtension(pluginPath);
            var domain = AppDomain.CreateDomain("Plugin Domain" + assemblyName, null, setup);
            var res = domain.CreateInstanceAndUnwrap(assemblyName, typeName) as IJobPlugin;
            */

            Assembly pluginAssembly;
            try
            {
                pluginAssembly = Assembly.LoadFrom(pluginPath);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error loading plugin assembly from file {0}.", pluginPath), ex);
                return;
            }

            var pluginTypes = from type in pluginAssembly.GetTypes()
                              where typeof(IJobPlugin).IsAssignableFrom(type)
                              select type;

            foreach (var pluginType in pluginTypes)
            {
                var instanceOfJobPlugin = (IJobPlugin)Activator.CreateInstance(pluginType);

                bool success = false;
                try
                {
                    instanceOfJobPlugin.Execute();
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error occured in {0}.", pluginType.Name), ex);
                }

                _producer.Publish(new JobExecutedMessage(Guid.NewGuid()) { Success = success, Timestamp = DateTime.UtcNow, Type = pluginType.Name });
            }
        }
    }
}
