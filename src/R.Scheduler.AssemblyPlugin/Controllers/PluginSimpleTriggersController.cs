using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
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
        [Route("api/plugins/{pluginName}/simpleTriggers")]
        public QueryResponse Post(string pluginName, [FromBody]PluginSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PluginSimpleTriggersController.Post(). PluginName = {0}", pluginName);

            var response = new QueryResponse { Valid = true};

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(pluginName);

            if (null == registeredPlugin)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredPluginNotFound",
                        Type = "Sender",
                        Message = string.Format("Error loading registered plugin {0}", model.PluginName)
                    }
                };

                return response;
            }

            try
            {
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
                }, typeof(PluginRunner));
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = "Server",
                        Message = string.Format("Error scheduling trigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
