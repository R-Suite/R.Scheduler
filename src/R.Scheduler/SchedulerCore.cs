using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Job;
using Quartz.Spi;
using R.Scheduler.Contracts.DataContracts;
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

        public void RegisterPlugin(string pluginName, string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                Logger.ErrorFormat("Error registering plugin {0}. Invalid assembly path {1}", pluginName, assemblyPath);
                return;
            }
            //todo: verify valid plugin.. reflection?

            _pluginStore.RegisterPlugin(new Plugin
            {
                AssemblyPath = assemblyPath,
                Name = pluginName,
                Status = "registered"
            });
        }

        public void RemovePlugin(string pluginName)
        {
            IScheduler sched = Scheduler.Instance();

            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(pluginName));

            foreach (var jobKey in jobKeys)
            {
                sched.DeleteJob(jobKey);
            }

            int result = _pluginStore.RemovePlugin(pluginName);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing  from data store. Plugin {0} not found", pluginName);
            }
        }

        public void DeschedulePlugin(string pluginName)
        {
            IScheduler sched = Scheduler.Instance();

            Quartz.Collection.ISet<TriggerKey> triggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(pluginName));
            sched.UnscheduleJobs(triggerKeys.ToList());

            // delete job if no triggers are left
            var jobKeys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(pluginName));

            foreach (var jobKey in jobKeys)
            {
                sched.DeleteJob(jobKey);
            }
        }
    }
}
