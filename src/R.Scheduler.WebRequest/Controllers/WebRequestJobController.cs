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
        public IEnumerable<Contracts.JobTypes.WebRequest.Model.WebRequestJob> Get()
        {
            Logger.Info("Entered WebRequestJobController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof(WebRequestJob));

            return jobDetails.Select(jobDetail =>
                                                    new Contracts.JobTypes.WebRequest.Model.WebRequestJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Uri = jobDetail.JobDataMap.GetString("uri"),
                                                        ContentType = "text/plain"
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/webRequests")]
        public Contracts.JobTypes.WebRequest.Model.WebRequestJob Get(string jobName, string jobGroup)
        {
            Logger.Info("Entered WebRequestJobController.Get().");

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

            return new Contracts.JobTypes.WebRequest.Model.WebRequestJob
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                Uri = jobDetail.JobDataMap.GetString("uri"),
                ActionType = jobDetail.JobDataMap.GetString("actionType"),
                Method = jobDetail.JobDataMap.GetString("method"),
                Body = jobDetail.JobDataMap.GetString("body"),
                ContentType = jobDetail.JobDataMap.GetString("contenType")
            };
        }

        /// <summary>
        /// Create new FtpJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/webRequests")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.WebRequest.Model.WebRequestJob model)
        {
            Logger.InfoFormat("Entered WebRequestJobController.Post(). Job Name = {0}", model.JobName);

            var dataMap = new Dictionary<string, object>
            {
                {"uri", model.Uri},
                {"actionType", model.ActionType},
                {"method", model.Method},
                {"body", model.Body},
                {"contentType", model.ContentType}
            };

            return base.CreateJob(model, typeof(WebRequestJob), dataMap);
        }
    }
}
