using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.JobRunners;

namespace R.Scheduler.Handlers
{
    public class ExecutePluginHandler : IMessageHandler<ExecutePlugin>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginStore;

        public ExecutePluginHandler(IPluginStore pluginStore)
        {
            _pluginStore = pluginStore;
        }

        public void Execute(ExecutePlugin message)
        {
            var registeredPlugin = _pluginStore.GetRegisteredPlugin(message.PluginName);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", message.PluginName);
                return;
            }

            IScheduler sched = Scheduler.Instance();
            IJobDetail jobDetail = new JobDetailImpl { JobDataMap = new JobDataMap { { "pluginPath", registeredPlugin.AssemblyPath} } };
            var tfb = new TriggerFiredBundle(jobDetail, null, null, false, null, null, null, null);
            IJobExecutionContext context = new JobExecutionContextImpl(sched, tfb, null);

            var pluginRunner = new PluginRunner();
            pluginRunner.Execute(context);
        }

        public IConsumeContext Context { get; set; }
    }
}
