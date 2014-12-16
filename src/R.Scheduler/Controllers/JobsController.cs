using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.JobTypes;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class JobsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public JobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs regardless of the job type
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobs")]
        public IEnumerable<BaseJob> Get()
        {
            Logger.Info("Entered JobsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails();

            return jobDetails.Select(jobDetail =>
                                                    new BaseJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        JobType = jobDetail.JobType.Name,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                    }).ToList();

        }

        /// <summary>
        /// Schedules a temporary job for an immediate execution
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/jobs/execution")]
        public QueryResponse Execute(string jobName, string jobGroup)
        {
            Logger.InfoFormat("Entered JobsController.Execute(). jobName = {0}, jobGroup = {1}", jobName, jobGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.ExecuteJob(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorExecutingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Remove job and all associated triggers.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/jobs")]
        public QueryResponse Delete(string jobName, string jobGroup)
        {
            Logger.InfoFormat("Entered JobsController.Delete(). jobName = {0}, jobGroup = {1}", jobName, jobGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJob(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                string type = "Server";
                if (ex is ArgumentException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingJob",
                        Type = type,
                        Message = string.Format("Error removing job {0}.", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
