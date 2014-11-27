using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.JobTypes.PipesAndFilters.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public class PafTransformationsController : BaseJobsImpController
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
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [Route("api/plugins")]
        public PafTransformation GetJob(string jobName, string jobGroup = null)
        {
            Logger.Info("Entered PafTransformationsController.Get().");

            IJobDetail jobDetail;

            try
            {
                jobDetail = _schedulerCore.GetJobDetail(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetail: {0}", ex.Message));
                return null;
            }

            return new PafTransformation
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                JobDefinitionPath = jobDetail.JobDataMap.GetString("jobDefinitionPath")
            };
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

            var dataMap = new Dictionary<string, object>
            {
                {"jobDefinitionPath", model.JobDefinitionPath},
            };

            return base.CreateJob(model, typeof(PafTransformationJob), dataMap);
        }
    }
}
