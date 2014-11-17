using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public class PafSimpleTriggersController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public PafSimpleTriggersController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/{id}/simpleTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PafSimpleTriggersController.Post(). PluginName = {0}", model.Name);

            const string jobType = "PipesAndFilters";

            var response = new QueryResponse { Valid = true};

            ICustomJob registeredCustomJob = base.GetRegisteredCustomJob(id, jobType);

            if (null == registeredCustomJob)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("Error loading registered  {0} Job {1}", jobType, model.Name)
                    }
                };

                return response;
            }

            try
            {
                _schedulerCore.ScheduleTrigger(new SimpleTrigger
                {
                    Name = model.TriggerName,
                    Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : registeredCustomJob.Id + "_Group",
                    JobName = !string.IsNullOrEmpty(model.JobName) ? model.JobName : registeredCustomJob.Id + "_JobName",
                    JobGroup = registeredCustomJob.Id.ToString(),
                    RepeatCount = model.RepeatCount,
                    RepeatInterval = model.RepeatInterval,
                    StartDateTime = model.StartDateTime,
                    DataMap = new Dictionary<string, object> { { "jobDefinitionPath", registeredCustomJob.Params } }
                }, typeof(JobRunner));
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
