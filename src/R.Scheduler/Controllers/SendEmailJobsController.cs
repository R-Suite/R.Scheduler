using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using Quartz.Job;
using R.Scheduler.Contracts.JobTypes.Email.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    /// <summary>
    /// Controller for a Quartz.net built-in job which sends an e-mail with the configured content to the configured
    /// </summary>
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class SendEmailJobsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected SendEmailJobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="SendMailJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/emails")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<EmailJob> Get()
        {
            Logger.Debug("Entered SendEmailJobsController.Get().");

            var authorizedJobGroups = PermissionsHelper.GetAuthorizedJobGroups();

            IDictionary<IJobDetail, Guid> jobDetailsMap;

            try
            {
                jobDetailsMap = _schedulerCore.GetJobDetails(authorizedJobGroups, typeof(SendMailJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetailsMap.Select(mapItem =>
                                                    new EmailJob
                                                    {
                                                        Id = mapItem.Value,
                                                        JobName = mapItem.Key.Key.Name,
                                                        JobGroup = mapItem.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Subject = mapItem.Key.JobDataMap.GetString("subject"),
                                                        Body = mapItem.Key.JobDataMap.GetString("message"),
                                                        CcRecipient = mapItem.Key.JobDataMap.GetString("cc_recipient"),
                                                        Encoding = mapItem.Key.JobDataMap.GetString("encoding"),
                                                        Password = mapItem.Key.JobDataMap.GetString("smtp_password"),
                                                        Recipient = mapItem.Key.JobDataMap.GetString("recipient"),
                                                        ReplyTo = mapItem.Key.JobDataMap.GetString("reply_to"),
                                                        Username = mapItem.Key.JobDataMap.GetString("smtp_username"),
                                                        SmtpHost = mapItem.Key.JobDataMap.GetString("smtp_host"),
                                                        SmtpPort = mapItem.Key.JobDataMap.GetString("smtp_port"),
                                                        Sender = mapItem.Key.JobDataMap.GetString("sender")
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="EmailJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/emails/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public EmailJob Get(Guid id)
        {
            Logger.Debug("Entered SendEmailJobsController.Get().");

            var authorizedJobGroups = PermissionsHelper.GetAuthorizedJobGroups();

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
                return new EmailJob
                {
                    Id = id,
                    JobName = jobDetail.Key.Name,
                    JobGroup = jobDetail.Key.Group,
                    SchedulerName = _schedulerCore.SchedulerName,
                    Subject = jobDetail.JobDataMap.GetString("subject"),
                    Body = jobDetail.JobDataMap.GetString("message"),
                    CcRecipient = jobDetail.JobDataMap.GetString("cc_recipient"),
                    Encoding = jobDetail.JobDataMap.GetString("encoding"),
                    Password = jobDetail.JobDataMap.GetString("smtp_password"),
                    Recipient = jobDetail.JobDataMap.GetString("recipient"),
                    ReplyTo = jobDetail.JobDataMap.GetString("reply_to"),
                    Username = jobDetail.JobDataMap.GetString("smtp_username"),
                    SmtpHost = jobDetail.JobDataMap.GetString("smtp_host"),
                    SmtpPort = jobDetail.JobDataMap.GetString("smtp_port"),
                    Sender = jobDetail.JobDataMap.GetString("sender"),
                    Description = jobDetail.Description
                };
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        }

        /// <summary>
        /// Create new <see cref="SendMailJob"/> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/emails")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]EmailJob model)
        {
            Logger.DebugFormat("Entered EmailsController.Post(). Job Name = {0}", model.JobName);

            var authorizedJobGroups = PermissionsHelper.GetAuthorizedJobGroups();

            if (string.IsNullOrEmpty(model.JobGroup))
                return CreateJob(model);

            if (authorizedJobGroups.Contains(model.JobGroup))
            {
                return CreateJob(model);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Update <see cref="SendMailJob"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/emails/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]EmailJob model)
        {
            Logger.DebugFormat("Entered EmailsController.Put(). Job Name = {0}", model.JobName);

            var authorizedJobGroups = PermissionsHelper.GetAuthorizedJobGroups();

            if (string.IsNullOrEmpty(model.JobGroup))
                return CreateJob(model);

            if (authorizedJobGroups.Contains(model.JobGroup))
            {
                return CreateJob(model);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        private QueryResponse CreateJob(EmailJob model)
        {
            var dataMap = new Dictionary<string, object>
            {
                {"message", model.Body},
                {"smtp_host", model.SmtpHost},
                {"smtp_port", model.SmtpPort},
                {"smtp_username", model.Username},
                {"smtp_password", model.Password},
                {"recipient", model.Recipient},
                {"cc_recipient", model.CcRecipient},
                {"sender", model.Sender},
                {"reply_to", model.ReplyTo},
                {"subject", model.Subject},
                {"encoding", model.Encoding}
            };

            return base.CreateJob(model, typeof (SendMailJob), dataMap, model.Description);
        }
    }
}
