using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using R.MessageBus.Interfaces;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Contracts.Messages;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Handlers
{
    public class GetPluginDetailsHandler : IMessageHandler<GetPluginDetailsRequest>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        private readonly ISchedulerCore _schedulerCore;

        public GetPluginDetailsHandler(IPluginStore pluginRepository, ISchedulerCore schedulerCore)
        {
            _pluginRepository = pluginRepository;
            _schedulerCore = schedulerCore;
        }

        public void Execute(GetPluginDetailsRequest message)
        {
            Logger.InfoFormat("Entered GetPluginDetailsHandler.Execute(). message.PluginName = {0}", message.PluginName);

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(message.PluginName);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", message.PluginName);
                return;
            }

            var pluginDetails = new PluginDetails
            {
                Name = registeredPlugin.Name,
                AssemblyPath = registeredPlugin.AssemblyPath,
                TriggerDetails = new List<TriggerDetails>()
            };

            var quartzTriggers = _schedulerCore.GetTriggersOfJobGroup(registeredPlugin.Name);

            foreach (var quartzTrigger in quartzTriggers)
            {
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();
                pluginDetails.TriggerDetails.Add(new TriggerDetails
                {
                    Description = quartzTrigger.Description,
                    StartTimeUtc = quartzTrigger.StartTimeUtc.UtcDateTime,
                    EndTimeUtc = (quartzTrigger.EndTimeUtc.HasValue) ? quartzTrigger.EndTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc = (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue) ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                });
            }

            Context.Reply(new GetPluginDetailsResponse(message.CorrelationId) { PluginDetails = pluginDetails });
        }

        public IConsumeContext Context { get; set; }
    }
}
