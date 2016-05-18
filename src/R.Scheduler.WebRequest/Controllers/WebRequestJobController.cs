using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.WebRequest.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class WebRequestJobController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected WebRequestJobController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="WebRequestJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/webRequests")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<Contracts.JobTypes.WebRequest.Model.WebRequestJob> Get()
        {
            Logger.Debug("Entered WebRequestJobController.Get().");

            var jobDetailsMap = _schedulerCore.GetJobDetails(typeof(WebRequestJob));

            return jobDetailsMap.Select(mapItem =>
                                                    new Contracts.JobTypes.WebRequest.Model.WebRequestJob
                                                    {
                                                        Id = mapItem.Value,
                                                        JobName = mapItem.Key.Key.Name,
                                                        JobGroup = mapItem.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Uri = mapItem.Key.JobDataMap.GetString("uri"),
                                                        ContentType = mapItem.Key.JobDataMap.GetString("contentType")
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="WebRequestJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/webRequests/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public Contracts.JobTypes.WebRequest.Model.WebRequestJob Get(Guid id)
        {
            Logger.Info("Entered WebRequestJobController.Get().");

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

            return new Contracts.JobTypes.WebRequest.Model.WebRequestJob
            {
                Id = id,
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                Uri = jobDetail.JobDataMap.GetString("uri"),
                ActionType = jobDetail.JobDataMap.GetString("actionType"),
                Method = jobDetail.JobDataMap.GetString("method"),
                Body = jobDetail.JobDataMap.GetString("body"),
                ContentType = jobDetail.JobDataMap.GetString("contentType"),
                Description = jobDetail.Description
            };
        }

        /// <summary>
        /// Create new <see cref="WebRequestJob"/> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/webRequests")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.WebRequest.Model.WebRequestJob model)
        {
            Logger.InfoFormat("Entered WebRequestJobController.Post(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        /// <summary>
        /// Update <see cref="WebRequestJob"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/webRequests/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]Contracts.JobTypes.WebRequest.Model.WebRequestJob model)
        {
            Logger.InfoFormat("Entered WebRequestJobController.Put(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        private QueryResponse CreateJob(Contracts.JobTypes.WebRequest.Model.WebRequestJob model)
        {
            var dataMap = new Dictionary<string, object>
            {
                {"uri", model.Uri},
                {"actionType", model.ActionType},
                {"method", model.Method},
                {"body", model.Body},
                {"contentType", model.ContentType}
            };

            return base.CreateJob(model, typeof (WebRequestJob), dataMap, model.Description);
        }
    }
}
