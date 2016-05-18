using System;
using System.Collections.Generic;
using System.Globalization;
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
using DirectoryScanJob = R.Scheduler.Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob;

namespace R.Scheduler.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
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
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob> Get()
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

            IDictionary<IJobDetail, Guid> jobDetailsMap;

            try
            {
                jobDetailsMap = _schedulerCore.GetJobDetails(typeof(NativeJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetailsMap.Select(jobDetail =>
                                                    new Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob
                                                    {
                                                        Id = jobDetail.Value,
                                                        JobName = jobDetail.Key.Key.Name,
                                                        JobGroup = jobDetail.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        DirectoryName = jobDetail.Key.JobDataMap.GetString("DIRECTORY_NAME"),
                                                        CallbackUrl = jobDetail.Key.JobDataMap.GetString("CALLBACK_URL"),
                                                        MinimumUpdateAge = jobDetail.Key.JobDataMap.GetLong("MINIMUM_UPDATE_AGE"),
                                                        LastModifiedTime = jobDetail.Key.JobDataMap.GetDateTime("LAST_MODIFIED_TIME"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/dirScanJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob Get(Guid id)
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

            IJobDetail jobDetail;

            try
            {
                jobDetail = _schedulerCore.GetJobDetail(id);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetail: {0}", ex.Message));
                return null;
            }

            return new Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob
            {
                Id = id,
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
        /// Create new <see cref="DirectoryScanJob" /> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/dirScanJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob model)
        {
            Logger.InfoFormat("Entered DirectoryScanJobsController.Post(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        /// <summary>
        /// Update <see cref="DirectoryScanJob" />
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/dirScanJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob model)
        {
            Logger.InfoFormat("Entered DirectoryScanJobsController.Put(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        private QueryResponse CreateJob(DirectoryScanJob model)
        {
            Uri uriResult;
            bool validUri = Uri.TryCreate(model.CallbackUrl, UriKind.Absolute, out uriResult) &&
                            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!validUri)
            {
                var response = new QueryResponse {Valid = false};
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
                {"MINIMUM_UPDATE_AGE", model.MinimumUpdateAge.ToString(CultureInfo.InvariantCulture)},
                {"LAST_MODIFIED_TIME", model.LastModifiedTime.ToString("o")}
            };

            return base.CreateJob(model, typeof (RDirectoryScanJob), dataMap);
        }
    }
}
