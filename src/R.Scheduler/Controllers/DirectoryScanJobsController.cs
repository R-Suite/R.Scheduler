using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using Quartz.Job;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.DirectoryScan;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class DirectoryScanJobsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public DirectoryScanJobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/dirScanJobs")]
        public IEnumerable<Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob> Get()
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

            IEnumerable<IJobDetail> jobDetails = null;

            try
            {
                jobDetails = _schedulerCore.GetJobDetails(typeof(NativeJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetails.Select(jobDetail =>
                                                    new Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        DirectoryName = jobDetail.JobDataMap.GetString("DIRECTORY_NAME"),
                                                        CallbackUrl = jobDetail.JobDataMap.GetString("CALLBACK_URL"),
                                                        MinimumUpdateAge = jobDetail.JobDataMap.GetLong("MINIMUM_UPDATE_AGE"),
                                                        LastModifiedTime = jobDetail.JobDataMap.GetDateTime("LAST_MODIFIED_TIME"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/dirScanJobs")]
        public Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob Get(string jobName, string jobGroup)
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

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

            return new Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                DirectoryName = jobDetail.JobDataMap.GetString("DIRECTORY_NAME"),
                CallbackUrl = jobDetail.JobDataMap.GetString("CALLBACK_URL"),
                MinimumUpdateAge = jobDetail.JobDataMap.GetLong("MINIMUM_UPDATE_AGE"),
                LastModifiedTime = jobDetail.JobDataMap.GetDateTime("LAST_MODIFIED_TIME")
            };
        }

        /// <summary>
        /// Create new SendMailJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/dirScanJobs")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob model)
        {
            Logger.InfoFormat("Entered DirectoryScanJobsController.Post(). Job Name = {0}", model.JobName);

            Uri uriResult;
            bool validUri = Uri.TryCreate(model.CallbackUrl, UriKind.Absolute, out uriResult) &&
                          (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!validUri)
            {
                var response = new QueryResponse { Valid = false };
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingJob",
                        Type = "Client",
                        Message = string.Format("Invalid CallbackUrl format: {0}", model.CallbackUrl)
                    }
                };

                return response;
            }

            var dataMap = new Dictionary<string, object>
            {
                {"DIRECTORY_NAME", model.DirectoryName},
                {"CALLBACK_URL", model.CallbackUrl},
                {"MINIMUM_UPDATE_AGE", model.MinimumUpdateAge.ToString()},
                {"LAST_MODIFIED_TIME", model.LastModifiedTime.ToString()},
                {"DIRECTORY_SCAN_LISTENER_NAME", "RDirectoryScanListener"}
            };

            return base.CreateJob(model, typeof(RDirectoryScanJob), dataMap);
        }
    }
}
