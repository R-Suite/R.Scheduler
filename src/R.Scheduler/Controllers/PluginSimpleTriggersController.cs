using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class PluginSimpleTriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;

        public PluginSimpleTriggersController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/simpleTriggers")]
        public void Post([FromBody]PluginSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PluginSimpleTriggersController.Post(). PluginName = {0}", model.PluginName);

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(model.PluginName);
            var pluginName = registeredPlugin.Name;

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", model.PluginName));

            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                Name = model.TriggerName,
                Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : pluginName + "_TriggerGroup",
                JobName = model.JobName,
                JobGroup = pluginName,

                RepeatCount = model.RepeatCount,
                RepeatInterval = model.RepeatInterval,
                StartDateTime = model.StartDateTime,
                DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
            });
        }
    }
}
