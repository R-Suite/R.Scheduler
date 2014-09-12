using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Interfaces;
using R.Scheduler.JobRunners;

namespace R.Scheduler.Handlers
{
    public class SchedulePluginWithSimpleTriggerHandler : IMessageHandler<SchedulePluginWithSimpleTrigger>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;
        private readonly IPluginStore _pluginStore;

        public SchedulePluginWithSimpleTriggerHandler(ISchedulerCore schedulerCore, IPluginStore pluginStore)
        {
            _schedulerCore = schedulerCore;
            _pluginStore = pluginStore;
        }

        public void Execute(SchedulePluginWithSimpleTrigger command)
        {
            Logger.InfoFormat("Entered SchedulePluginWithSimpleTriggerHandler.Execute(). PluginName = {0}", command.PluginName);

            var registeredPlugin = _pluginStore.GetRegisteredPlugin(command.PluginName);
            var pluginName = registeredPlugin.Name;

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", command.PluginName));

            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                Name = command.TriggerName,
                Group = !string.IsNullOrEmpty(command.TriggerGroup) ? command.TriggerGroup : pluginName + "_TriggerGroup",
                JobName = command.JobName,
                JobGroup = pluginName,
                RepeatCount = command.RepeatCount,
                RepeatInterval = command.RepeatInterval,
                StartDateTime = command.StartDateTime,
                DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
            }, typeof(PluginRunner));
        }

        public IConsumeContext Context { get; set; }
    }
}
