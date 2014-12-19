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
        /// Get all the jobs of type <see cref="FtpJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobs/ftp")]
        public IEnumerable<Contracts.JobTypes.Ftp.Model.FtpJob> Get()
        {
            Logger.Info("Entered FtpJobsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof(FtpJob));

            return jobDetails.Select(jobDetail =>
                                                    new Contracts.JobTypes.Ftp.Model.FtpJob
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
        [Route("api/jobs/ftp")]
        public Contracts.JobTypes.Ftp.Model.FtpJob Get(string jobName, string jobGroup)
        {
            Logger.Info("Entered FtpJobsController.Get().");

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

            return new Contracts.JobTypes.Ftp.Model.FtpJob
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                FtpHost = jobDetail.JobDataMap.GetString("ftpHost")
            };
        }

        /// <summary>
        /// Create new FtpJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/jobs/ftp")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.Ftp.Model.FtpJob model)
        {
            Logger.InfoFormat("Entered FtpJobsController.Post(). Job Name = {0}", model.JobName);

            var dataMap = new Dictionary<string, object>
            {
                {"ftpHost", model.FtpHost},
                {"action", model.Action},
                {"serverPort", model.ServerPort},
                {"userName", model.Username},
                {"password", model.Password},
                {"localDirectoryPath", model.LocalDirectoryPath},
                {"remotelDirectoryPath", model.RemotelDirectoryPath},
                {"fileName", model.FileName},
                {"fileExtension", model.FileExtension},
                {"cutOffTimeSpan", model.CutOffTimeSpan}
            };

            return base.CreateJob(model, typeof(FtpJob), dataMap);
        }
    }
}
