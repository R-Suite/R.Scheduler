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
    public class PluginCronTriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;

        public PluginCronTriggersController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/cronTriggers")]
        public void Post([FromBody]PluginCronTrigger model)
        {
            Logger.InfoFormat("Entered PluginCronTriggersController.Post(). PluginName = {0}", model.PluginName);

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(model.PluginName);
            var pluginName = registeredPlugin.Name;

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", model.PluginName));

            _schedulerCore.ScheduleTrigger(new CronTrigger
            {
                Name = model.TriggerName,
                Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : pluginName + "_TriggerGroup",
                JobName = model.JobName,
                JobGroup = pluginName,

                CronExpression = model.CronExpression,
                StartDateTime = model.StartDateTime,
                DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
            });
        }
    }
}
