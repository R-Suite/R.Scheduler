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

namespace R.Scheduler.Ftp.Controllers
{
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
        public IEnumerable<Contracts.JobTypes.Ftp.Model.FtpDownloadJob> Get()
        {
            Logger.Debug("Entered FtpJobsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof(FtpDownloadJob));

            return jobDetails.Select(jobDetail =>
                                                    new Contracts.JobTypes.Ftp.Model.FtpDownloadJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        FtpHost = jobDetail.JobDataMap.GetString("ftpHost"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/ftpDownloads")]
        public Contracts.JobTypes.Ftp.Model.FtpDownloadJob Get(string jobName, string jobGroup)
        {
            Logger.Debug("Entered FtpJobsController.Get().");

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

            return new Contracts.JobTypes.Ftp.Model.FtpDownloadJob
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                FtpHost = jobDetail.JobDataMap.GetString("ftpHost"),
                ServerPort = jobDetail.JobDataMap.GetString("serverPort"),
                Username = jobDetail.JobDataMap.GetString("userName"),
                Password = jobDetail.JobDataMap.GetString("password"),
                LocalDirectoryPath = jobDetail.JobDataMap.GetString("localDirectoryPath"),
                RemoteDirectoryPath = jobDetail.JobDataMap.GetString("remoteDirectoryPath"),
                FileExtensions = jobDetail.JobDataMap.GetString("fileExtensions"),
                CutOffTimeSpan = jobDetail.JobDataMap.GetString("cutOffTimeSpan"),
            };
        }

        /// <summary>
        /// Create new FtpJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/ftpDownloads")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.Ftp.Model.FtpDownloadJob model)
        {
            Logger.DebugFormat("Entered FtpJobsController.Post(). Job Name = {0}", model.JobName);

            var dataMap = new Dictionary<string, object>
            {
                {"ftpHost", model.FtpHost},
                {"serverPort", model.ServerPort},
                {"userName", model.Username},
                {"password", model.Password},
                {"localDirectoryPath", model.LocalDirectoryPath},
                {"remoteDirectoryPath", model.RemoteDirectoryPath},
                {"fileExtensions", model.FileExtensions},
                {"cutOffTimeSpan", model.CutOffTimeSpan}
            };

            return base.CreateJob(model, typeof(FtpDownloadJob), dataMap);
        }
    }
}
