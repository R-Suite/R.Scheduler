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
        [Route("api/plugins/{pluginName}/cronTriggers")]
        public QueryResponse Post(string pluginName, [FromBody]PluginCronTrigger model)
        {
            Logger.InfoFormat("Entered PluginCronTriggersController.Post(). PluginName = {0}", pluginName);

            var response = new QueryResponse { Valid = true };

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
                _schedulerCore.ScheduleTrigger(new CronTrigger
                {
                    Name = model.TriggerName,
                    Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : pluginName + "_TriggerGroup",
                    JobName = model.JobName,
                    JobGroup = pluginName,

                    CronExpression = model.CronExpression,
                    StartDateTime = model.StartDateTime,
                    DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
                }, typeof(PluginRunner));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error scheduling CronTrigger  {0}. {1}", model.TriggerName, ex.Message);

                string type = "Server";
                
                if (ex is FormatException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = type,
                        Message = string.Format("Error scheduling CronTrigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
