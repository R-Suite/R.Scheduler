using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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
        private readonly IPermissionsHelper _permissionsHelper;

        public DirectoryScanJobsController(IPermissionsHelper permissionsHelper, ISchedulerCore schedulerCore) : base(schedulerCore)
        {
            _schedulerCore = schedulerCore;
            _permissionsHelper = permissionsHelper;
        }

        /// <summary>
        /// Get all the jobs of type <see cref="Contracts.JobTypes.DirectoryScan.Model.DirectoryScanJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/dirScanJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<DirectoryScanJob> Get()
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups();

            IDictionary<IJobDetail, Guid> jobDetailsMap;

            try
            {
                jobDetailsMap = _schedulerCore.GetJobDetails(authorizedJobGroups, typeof(NativeJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetailsMap.Select(jobDetail =>
                                                    new DirectoryScanJob
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
        public DirectoryScanJob Get(Guid id)
        {
            Logger.Info("Entered DirectoryScanJobsController.Get().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

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

            if (jobDetail != null &&
                (authorizedJobGroups.Contains(jobDetail.Key.Group) || authorizedJobGroups.Contains("*")))
            {
                return new DirectoryScanJob
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
            if (jobDetail == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Create new <see cref="DirectoryScanJob" /> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/dirScanJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]DirectoryScanJob model)
        {
            Logger.InfoFormat("Entered DirectoryScanJobsController.Post(). Job Name = {0}", model.JobName);

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            if (string.IsNullOrEmpty(model.JobGroup))
                return CreateJob(model);

            if ((authorizedJobGroups.Contains(model.JobGroup) || authorizedJobGroups.Contains("*")) && model.JobGroup != "*")
            {
                return CreateJob(model);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Update <see cref="DirectoryScanJob" />
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/dirScanJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]DirectoryScanJob model)
        {
            Logger.InfoFormat("Entered DirectoryScanJobsController.Put(). Job Name = {0}", model.JobName);

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            if (string.IsNullOrEmpty(model.JobGroup))
                return CreateJob(model);

            if ((authorizedJobGroups.Contains(model.JobGroup) || authorizedJobGroups.Contains("*")) && model.JobGroup != "*")
            {
                return CreateJob(model);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
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

            return base.CreateJob(model, typeof (RDirectoryScanJob), dataMap, model.Description);
        }
    }
}
