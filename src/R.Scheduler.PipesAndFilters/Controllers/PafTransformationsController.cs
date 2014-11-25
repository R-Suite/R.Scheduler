using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.JobTypes.PipesAndFilters.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public class PafTransformationsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public PafTransformationsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="PafTransformationJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/pipesandfilters")]
        public IEnumerable<PafTransformation> Get()
        {
            Logger.Info("Entered PafTransformationsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof(PafTransformationJob));

            return jobDetails.Select(jobDetail =>
                                                    new PafTransformation
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        JobDefinitionPath = jobDetail.JobDataMap.GetString("jobDefinitionPath"),
                                                    }).ToList();

        }

        /// <summary>
        /// Create new PafTransformationJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/pipesandfilters")]
        public QueryResponse Post([FromBody]PafTransformation model)
        {
            Logger.InfoFormat("Entered PafTransformationsController.Post(). Job Name = {0}", model.JobName);

            var response = new QueryResponse { Valid = true };

            var dataMap = new Dictionary<string, object>
            {
                {"jobDefinitionPath", model.JobDefinitionPath},
            };

            try
            {
                _schedulerCore.CreateJob(model.JobName, model.JobGroup, typeof(PafTransformationJob), dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
