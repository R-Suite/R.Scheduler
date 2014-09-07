using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Job;
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

            IJobDetail jobDetail = new JobDetailImpl { JobDataMap = new JobDataMap { { "pluginPath", registeredPlugin.AssemblyPath} } };
            IOperableTrigger trigger = new SimpleTriggerImpl("AdHockTrigger") { RepeatCount = 1};
            var tfb = new TriggerFiredBundle(jobDetail, trigger, null, false, null, null, null, null);

            IJob job = new NoOpJob();

            IJobExecutionContext context = new JobExecutionContextImpl(null, tfb, job);

            var pluginRunner = new PluginRunner();

            pluginRunner.Execute(context);
        }

        public IConsumeContext Context { get; set; }
    }
}
