using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using FeatureToggle.Core;
using Quartz;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Core.FeatureToggles;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Ftp.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class FtpJobsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected FtpJobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="FtpDownloadJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/ftpDownloads")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<Contracts.JobTypes.Ftp.Model.FtpDownloadJob> Get()
        {
            Logger.Debug("Entered FtpJobsController.Get().");

            var jobDetailsMap = _schedulerCore.GetJobDetails(typeof(FtpDownloadJob));

            return jobDetailsMap.Select(mapItem =>
                                                    new Contracts.JobTypes.Ftp.Model.FtpDownloadJob
                                                    {
                                                        Id = mapItem.Value,
                                                        JobName = mapItem.Key.Key.Name,
                                                        JobGroup = mapItem.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        FtpHost = mapItem.Key.JobDataMap.GetString("ftpHost"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="FtpDownloadJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/ftpDownloads/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public Contracts.JobTypes.Ftp.Model.FtpDownloadJob Get(Guid id)
        {
            Logger.Debug("Entered FtpJobsController.Get().");

            IJobDetail jobDetail;

            try
            {
                jobDetail = _schedulerCore.GetJobDetail(id);
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Error getting JobDetail: {0}", ex.Message);
                return null;
            }

            string username = jobDetail.JobDataMap.GetString("userName");
            string password = jobDetail.JobDataMap.GetString("password");

            try
            {
                if (new EncryptionFeatureToggle().FeatureEnabled)
                {
                    username = AESGCM.SimpleDecrypt(username, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                    password = AESGCM.SimpleDecrypt(password, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigurationError getting FtpDownload job.", ex);
            }

            return new Contracts.JobTypes.Ftp.Model.FtpDownloadJob
            {
                Id = id,
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                FtpHost = jobDetail.JobDataMap.GetString("ftpHost"),
                ServerPort = jobDetail.JobDataMap.GetString("serverPort"),
                Username = username,
                Password = password,
                LocalDirectoryPath = jobDetail.JobDataMap.GetString("localDirectoryPath"),
                RemoteDirectoryPath = jobDetail.JobDataMap.GetString("remoteDirectoryPath"),
                FileExtensions = jobDetail.JobDataMap.GetString("fileExtensions"),
                CutOffTimeSpan = jobDetail.JobDataMap.GetString("cutOffTimeSpan"),
                Description = jobDetail.Description
            };
        }

        /// <summary>
        /// Create new <see cref="FtpDownloadJob"/> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/ftpDownloads")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.Ftp.Model.FtpDownloadJob model)
        {
            Logger.DebugFormat("Entered FtpJobsController.Post(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        /// <summary>
        /// Update <see cref="FtpDownloadJob"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/ftpDownloads/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]Contracts.JobTypes.Ftp.Model.FtpDownloadJob model)
        {
            Logger.DebugFormat("Entered FtpJobsController.Put(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        private QueryResponse CreateJob(Contracts.JobTypes.Ftp.Model.FtpDownloadJob model)
        {
            string username = model.Username;
            string password = model.Password;

            try
            {
                if (new EncryptionFeatureToggle().FeatureEnabled)
                {
                    username = AESGCM.SimpleEncrypt(username, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                    password = AESGCM.SimpleEncrypt(password, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigurationError creating FtpDownload job.", ex);
            }

            var dataMap = new Dictionary<string, object>
            {
                {"ftpHost", model.FtpHost},
                {"serverPort", model.ServerPort},
                {"userName", username},
                {"password", password},
                {"localDirectoryPath", model.LocalDirectoryPath},
                {"remoteDirectoryPath", model.RemoteDirectoryPath},
                {"fileExtensions", model.FileExtensions},
                {"cutOffTimeSpan", model.CutOffTimeSpan}
            };

            return base.CreateJob(model, typeof (FtpDownloadJob), dataMap, model.Description);
        }
    }
}
