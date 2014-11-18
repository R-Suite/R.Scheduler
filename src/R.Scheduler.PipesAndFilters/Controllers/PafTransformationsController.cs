using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.JobTypes.PipesAndFilters.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public class PafTransformationsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _repository;
        private const string JobType = "PaF";

        public PafTransformationsController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
        }

        [Route("api/pipesandfilters")]
        public IEnumerable<PafTransformation> Get()
        {
            Logger.Info("Entered PafTransformationsController.Get().");

            IList<ICustomJob> registeredJobs = _repository.GetRegisteredJobs(JobType);

            return registeredJobs.Select(registeredJob =>   new PafTransformation
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
        /// <returns>QueryResponse</returns>
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/execute")]
        public QueryResponse Execute([FromBody]string model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Execute(). name = {0}", model);

            return ExecuteCustomJob(model, JobType, "jobDefinitionPath", typeof(JobRunner));
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="model">PafTransformation name</param>
        /// <returns>QueryResponse</returns>
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/deschedule")]
        public QueryResponse Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Deschedule(). name = {0}", model);

            return DescheduleCustomJob(model, JobType);
        }

        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters")]
        public QueryResponse Post([FromBody]PafTransformation model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Post(). name = {0}", model.Name);

            return RegisterCustomJob(new CustomJob { Name = model.Name, Params = model.JobDefinitionPath, JobType = JobType });
        }

        [AcceptVerbs("PUT")]
        [Route("api/pipesandfilters/{id}")]
        public QueryResponse Put(string id, [FromBody]PafTransformation model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Put(). name = {0}", model.Name);

            return UpdateCustomJob(id, new CustomJob { Name = model.Name });
        }

        [Route("api/pipesandfilters/{id}")]
        public PafTransformationDetails Get(string id)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Get(). id = {0}", id);

            ICustomJob registeredJob = base.GetRegisteredCustomJob(id, JobType);

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

            retval.TriggerDetails = GetCustomJobTriggerDetails(registeredJob);

            return retval;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/pipesandfilters")]
        public QueryResponse Delete(string id)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Delete(). id = {0}", id);

            return DeleteCustomJob(id, JobType);
        }

        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/{id}/simpleTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Post(). Name = {0}", model.Name);

            return CreateCustomJobSimpleTrigger(id, model, JobType, "jobDefinitionPath", typeof(JobRunner));
        }

        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters/{id}/cronTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobCronTrigger model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Post(). Name = {0}", model.Name);

            return CreateCustomJobCronTrigger(id, model, JobType, "jobDefinitionPath", typeof(JobRunner));
        }
    }
}
