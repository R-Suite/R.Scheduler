using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using Quartz.Job;
using R.Scheduler.Contracts.JobTypes.Email.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
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
        [Route("api/emails")]
        public IEnumerable<EmailJob> Get()
        {
            Logger.Info("Entered SendEmailJobsController.Get().");

            IEnumerable<IJobDetail> jobDetails = null;

            try
            {
                jobDetails = _schedulerCore.GetJobDetails(typeof(SendMailJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetails.Select(jobDetail =>
                                                    new EmailJob
                                                    {
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
                                                        Sender = jobDetail.JobDataMap.GetString("sender")
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [Route("api/emails")]
        public EmailJob GetJob(string jobName, string jobGroup = null)
        {
            Logger.Info("Entered SendEmailJobsController.Get().");

            IJobDetail jobDetail = null;

            try
            {
                jobDetail = _schedulerCore.GetJobDetail(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetail: {0}", ex.Message));
                return null;
            }

            return new EmailJob
            {
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
                Sender = jobDetail.JobDataMap.GetString("sender")
            };
        }

        /// <summary>
        /// Create new SendMailJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/emails")]
        public QueryResponse Post([FromBody]EmailJob model)
        {
            Logger.InfoFormat("Entered EmailsController.Post(). Job Name = {0}", model.JobName);

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

            return base.CreateJob(model, typeof(SendMailJob), dataMap);
        }
    }
}
