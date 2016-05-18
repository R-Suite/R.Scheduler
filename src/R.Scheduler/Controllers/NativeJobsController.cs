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
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
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
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<NativeExecJob> Get()
        {
            Logger.Debug("Entered NativeJobsController.Get().");

            IDictionary<IJobDetail, Guid> jobDetailsMap;

            try
            {
                jobDetailsMap = _schedulerCore.GetJobDetails(typeof(NativeJob));
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Error getting JobDetails: {0}", ex.Message));
                return null;
            }

            return jobDetailsMap.Select(mapItem =>
                                                    new NativeExecJob
                                                    {
                                                        Id = mapItem.Value,
                                                        JobName = mapItem.Key.Key.Name,
                                                        JobGroup = mapItem.Key.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Command = mapItem.Key.JobDataMap.GetString("command"),
                                                        Parameters = mapItem.Key.JobDataMap.GetString("parameters"),
                                                        WaitForProcess = mapItem.Key.JobDataMap.GetBooleanValueFromString("waitForProcess"),
                                                        ConsumeStreams = mapItem.Key.JobDataMap.GetBooleanValueFromString("consumeStreams"),
                                                        WorkingDirectory = mapItem.Key.JobDataMap.GetString("workingDirectory"),
                                                    }).ToList();

        }

        /// <summary>
        /// Get job details of <see cref="NativeExecJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/nativeJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public NativeExecJob Get(Guid id)
        {
            Logger.Debug("Entered NativeJobsController.Get().");

            IJobDetail jobDetail;

            try
            {
                jobDetail = _schedulerCore.GetJobDetail(id);
            }
            catch (Exception ex)
            {
                Logger.Debug(string.Format("Error getting JobDetail: {0}", ex.Message));
                return null;
            }

            return new NativeExecJob
            {
                Id = id,
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                Command = jobDetail.JobDataMap.GetString("command"),
                Parameters = jobDetail.JobDataMap.GetString("parameters"),
                WaitForProcess = jobDetail.JobDataMap.GetBooleanValueFromString("waitForProcess"),
                ConsumeStreams = jobDetail.JobDataMap.GetBooleanValueFromString("consumeStreams"),
                WorkingDirectory = jobDetail.JobDataMap.GetString("workingDirectory"),
                Description = jobDetail.Description
            };
        }

        /// <summary>
        /// Create new <see cref="NativeExecJob"/> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/nativeJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]NativeExecJob model)
        {
            Logger.DebugFormat("Entered NativeJobsController.Post(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        /// <summary>
        /// Update <see cref="NativeExecJob"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/nativeJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]NativeExecJob model)
        {
            Logger.DebugFormat("Entered NativeJobsController.Put(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        private QueryResponse CreateJob(NativeExecJob model)
        {
            var dataMap = new Dictionary<string, object>
            {
                {"command", model.Command},
                {"parameters", model.Parameters},
                {"waitForProcess", model.WaitForProcess.ToString()},
                {"consumeStreams", model.ConsumeStreams.ToString()},
                {"workingDirectory", model.WorkingDirectory}
            };

            return base.CreateJob(model, typeof (NativeJob), dataMap, model.Description);
        }
    }
}
