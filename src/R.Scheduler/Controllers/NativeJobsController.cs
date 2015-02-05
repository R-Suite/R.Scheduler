using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using Quartz.Job;
using R.Scheduler.Contracts.JobTypes.Native.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    /// <summary>
    /// Controller for Quartz.net built-in job for executing native executables in a separate process.
    /// </summary>
    public class NativeJobsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected NativeJobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="SendMailJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/nativeJobs")]
        public IEnumerable<NativeExecJob> Get()
        {
            Logger.Info("Entered NativeJobsController.Get().");

            IEnumerable<IJobDetail> jobDetails = null;

            try
            {
                jobDetails = _schedulerCore.GetJobDetails(typeof(NativeJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetails.Select(jobDetail =>
                                                    new NativeExecJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Command = jobDetail.JobDataMap.GetString("command"),
                                                        Parameters = jobDetail.JobDataMap.GetString("parameters"),
                                                        WaitForProcess = jobDetail.JobDataMap.GetBooleanValueFromString("waitForProcess"),
                                                        ConsumeStreams = jobDetail.JobDataMap.GetBooleanValueFromString("consumeStreams"),
                                                        WorkingDirectory = jobDetail.JobDataMap.GetString("workingDirectory"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="jobName"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/nativeJobs")]
        public NativeExecJob Get(string jobName, string jobGroup)
        {
            Logger.Info("Entered NativeJobsController.Get().");

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

            return new NativeExecJob
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                Command = jobDetail.JobDataMap.GetString("command"),
                Parameters = jobDetail.JobDataMap.GetString("parameters"),
                WaitForProcess = jobDetail.JobDataMap.GetBooleanValueFromString("waitForProcess"),
                ConsumeStreams = jobDetail.JobDataMap.GetBooleanValueFromString("consumeStreams"),
                WorkingDirectory = jobDetail.JobDataMap.GetString("workingDirectory")
            };
        }

        /// <summary>
        /// Create new SendMailJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/nativeJobs")]
        public QueryResponse Post([FromBody]NativeExecJob model)
        {
            Logger.InfoFormat("Entered NativeJobsController.Post(). Job Name = {0}", model.JobName);

            var dataMap = new Dictionary<string, object>
            {
                {"command", model.Command},
                {"parameters", model.Parameters},
                {"waitForProcess", model.WaitForProcess.ToString()},
                {"consumeStreams", model.ConsumeStreams.ToString()},
                {"workingDirectory", model.WorkingDirectory}
            };

            return base.CreateJob(model, typeof(NativeJob), dataMap);
        }
    }
}
