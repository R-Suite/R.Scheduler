using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class AssemblyPluginsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISchedulerCore _schedulerCore;
        private readonly IPermissionsHelper _permissionsHelper;

        protected AssemblyPluginsController(IPermissionsHelper permissionsHelper, ISchedulerCore schedulerCore) : base(schedulerCore)
        {
            _permissionsHelper = permissionsHelper;
            _schedulerCore = schedulerCore;
        }

        /// <summary>
        /// Get all the jobs of type <see cref="AssemblyPluginJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/plugins")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<PluginJob> Get()
        {
            Logger.Debug("Entered AssemblyPluginsController.Get().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups();

            IDictionary<IJobDetail, Guid> jobDetailsMap;
            
            try
            {
                jobDetailsMap = _schedulerCore.GetJobDetails(authorizedJobGroups, typeof(AssemblyPluginJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetailsMap.Select(mapItem =>
                                                    new PluginJob
                                                    {
                                                        Id = mapItem.Value,
                                                        JobName = mapItem.Key.Key.Name,
                                                        JobGroup = mapItem.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        AssemblyPath = mapItem.Key.JobDataMap.GetString("pluginPath"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="PluginJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/plugins/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public PluginJob Get(Guid id)
        {
            Logger.Debug("Entered AssemblyPluginsController.Get().");

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
                return new PluginJob
                {
                    Id = id,
                    JobName = jobDetail.Key.Name,
                    JobGroup = jobDetail.Key.Group,
                    SchedulerName = _schedulerCore.SchedulerName,
                    AssemblyPath = jobDetail.JobDataMap.GetString("pluginPath"),
                    Description = jobDetail.Description
                };
            }
            if (jobDetail == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Create new AssemblyPluginJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]PluginJob model)
        {
            Logger.DebugFormat("Entered AssemblyPluginsController.Post(). Job Name = {0}", model.JobName);
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
        /// Update AssemblyPluginJob
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/plugins/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]PluginJob model)
        {
            Logger.DebugFormat("Entered AssemblyPluginsController.Put(). Job Name = {0}", model.JobName);

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            if (string.IsNullOrEmpty(model.JobGroup))
                return CreateJob(model);

            if ((authorizedJobGroups.Contains(model.JobGroup) || authorizedJobGroups.Contains("*")) && model.JobGroup != "*")
            {
                return CreateJob(model);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        private QueryResponse CreateJob(PluginJob model)
        {
            var dataMap = new Dictionary<string, object>
            {
                {"pluginPath", model.AssemblyPath},
            };

            return base.CreateJob(model, typeof (AssemblyPluginJob), dataMap, model.Description);
        }
    }
}
