using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Job;
using Quartz.Spi;
using R.Scheduler.Interfaces;
using R.Scheduler.JobRunners;

namespace R.Scheduler
{
    public class SchedulerCore : ISchedulerCore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginStore;

        public SchedulerCore(IPluginStore pluginStore)
        {
            _pluginStore = pluginStore;
        }

        public void ExecutePlugin(string pluginName)
        {
            var registeredPlugin = _pluginStore.GetRegisteredPlugin(pluginName);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", pluginName);
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
    }
}
