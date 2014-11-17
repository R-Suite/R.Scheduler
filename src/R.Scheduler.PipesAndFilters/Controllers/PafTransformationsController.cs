using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using Quartz;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using R.Scheduler.PipesAndFilters.Contracts;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public class PafTransformationsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _repository;
        readonly ISchedulerCore _schedulerCore;
        readonly IJobTypeManager _pafTransformationManager;

        public PafTransformationsController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
            _pafTransformationManager = ObjectFactory.GetInstance<IJobTypeManager>();
        }

        private const string JobType = "PaF";

        // GET api/values 
        [Route("api/pipesandfilters")]
        public IEnumerable<PafTransformation> Get()
        {
            Logger.Info("Entered PafTransformationsController.Get().");

            IList<ICustomJob> registeredJobs = _repository.GetRegisteredJobs(JobType);

            return
                registeredJobs.Select(
                    registeredJob =>
                        new PafTransformation
                        {
                            Name = registeredJob.Name,
                            JobDefinitionPath = registeredJob.Params,
                            Id = registeredJob.Id
                        }).ToList();
        }

        /// <summary>
        /// Schedules a temporary job for an immediate execution
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/execute")]
        public QueryResponse Execute([FromBody]string model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Execute(). name = {0}", model);

            var response = ExecuteCustomJob(model, JobType, "jobDefinitionPath", typeof(JobRunner));

            return response;
        }


        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="model">PafTransformation name</param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/deschedule")]
        public QueryResponse Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Deschedule(). name = {0}", model);

            ICustomJob registeredJob = base.GetRegisteredCustomJob(model, JobType);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJobGroup(registeredJob.Id.ToString());
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingJobGroup",
                        Type = "Server",
                        Message = string.Format("Error:{0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // POST api/plugins 
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters")]
        public QueryResponse Post([FromBody]PafTransformation model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Post(). name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _pafTransformationManager.Register(model.Name, model.JobDefinitionPath);
            }
            catch (Exception ex)
            {
                string type = "Server";
                if (ex is FileNotFoundException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRegisteringPafTransformation",
                        Type = type,
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // PUT api/plugins/{id}
        [AcceptVerbs("PUT")]
        [Route("api/pipesandfilters/{id}")]
        public QueryResponse Put(string id, [FromBody]PafTransformation model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Put(). name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _repository.UpdateName(new Guid(id), model.Name);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorUpdatingPafTransformation",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // GET api/values 
        [Route("api/pipesandfilters/{id}")]
        public PafTransformationDetails Get(string id)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Get(). id = {0}", id);

            ICustomJob registeredJob = base.GetRegisteredCustomJob(id, JobType);

            // Still couldn't get, return null
            if (null == registeredJob)
            {
                Logger.ErrorFormat("Error getting registered PafTransformation {0}", id);
                return null;
            }

            var retval = new PafTransformationDetails
            {
                Name = registeredJob.Name,
                JobDefinitionPath = registeredJob.Params,
                TriggerDetails = new List<TriggerDetails>()
            };

            var quartzTriggers = _schedulerCore.GetTriggersOfJobGroup(registeredJob.Id.ToString());

            foreach (ITrigger quartzTrigger in quartzTriggers)
            {
                var triggerType = string.Empty;
                if (quartzTrigger is ICronTrigger)
                {
                    triggerType = "Cron";
                }
                if (quartzTrigger is ISimpleTrigger)
                {
                    triggerType = "Simple";
                }
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();
                retval.TriggerDetails.Add(new TriggerDetails
                {
                    Name = quartzTrigger.Key.Name,
                    Group = quartzTrigger.Key.Group,
                    JobName = quartzTrigger.JobKey.Name,
                    JobGroup = quartzTrigger.JobKey.Group,
                    Description = quartzTrigger.Description,
                    StartTimeUtc = quartzTrigger.StartTimeUtc.UtcDateTime,
                    EndTimeUtc = (quartzTrigger.EndTimeUtc.HasValue) ? quartzTrigger.EndTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc = (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue) ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    Type = triggerType
                });
            }

            return retval;
        }

        // DELETE api/plugins/id 
        [AcceptVerbs("DELETE")]
        [Route("api/pipesandfilters")]
        public QueryResponse Delete(string id)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Delete(). id = {0}", id);

            var registeredJob = base.GetRegisteredCustomJob(id, JobType);

            var response = new QueryResponse { Valid = true };

            _schedulerCore.RemoveJobGroup(registeredJob.Id.ToString());

            int result = _repository.Remove(registeredJob.Id);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing from data store. PafTransformation {0} not found", id);

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredPafTransformationNotFound",
                        Type = "Sender",
                        Message = string.Format("PafTransformation {0} not found", id)
                    }
                };
            }

            return response;
        }
    }
}
