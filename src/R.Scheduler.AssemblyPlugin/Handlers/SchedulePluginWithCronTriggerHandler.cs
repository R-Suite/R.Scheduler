using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class SchedulePluginWithCronTriggerHandler : IMessageHandler<SchedulePluginWithCronTrigger>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;
        private readonly IPluginStore _pluginStore;

        public SchedulePluginWithCronTriggerHandler(ISchedulerCore schedulerCore, IPluginStore pluginStore)
        {
            _schedulerCore = schedulerCore;
            _pluginStore = pluginStore;
        }

        public void Execute(SchedulePluginWithCronTrigger command)
        {
            Logger.InfoFormat("Entered SchedulePluginWithCronTriggerHandler.Execute(). PluginName = {0}", command.PluginName);

            if (string.IsNullOrEmpty(command.CronExpression))
                throw new ArgumentException(string.Format("CronExpression is empty for {0}", command.PluginName));

            var registeredPlugin = _pluginStore.GetRegisteredPlugin(command.PluginName);
            var pluginName = registeredPlugin.Name;

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", pluginName));

            _schedulerCore.ScheduleTrigger(new CronTrigger
            {
                Name = command.TriggerName,
                Group = !string.IsNullOrEmpty(command.TriggerGroup) ? command.TriggerGroup : pluginName + "_TriggerGroup",
                JobName = command.JobName,
                JobGroup = pluginName,
                CronExpression = command.CronExpression,
                StartDateTime = command.StartDateTime,
                DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
            }, typeof(PluginRunner));
        }

        public IConsumeContext Context { get; set; }
    }
}
