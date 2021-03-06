﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using AutoMapper;
using Common.Logging;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class AnalyticsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAnalytics _analytics;
        private readonly IPermissionsHelper _permissionsHelper;

        public AnalyticsController(IAnalytics analytics, IPermissionsHelper permissionsHelper)
        {
            _permissionsHelper = permissionsHelper;
            _analytics = analytics;
            Mapper.CreateMap<AuditLog, FireInstance>();
        }

        /// <summary>
        /// Get count of all the jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobCount")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public int GetJobCount()
        {
            Logger.Debug("Entered AnalyticsController.GetJobCount().");

            return _analytics.GetJobCount();
        }

        /// <summary>
        /// Get count of all the triggers
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/triggerCount")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public int GetTriggerCount()
        {
            Logger.Debug("Entered AnalyticsController.GetTriggerCount().");

            return _analytics.GetTriggerCount();
        }

        /// <summary>
        /// Get executing jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/executingJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetExecutingJobs()
        {
            Logger.Debug("Entered AnalyticsController.GetExecutingJobs().");
            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups();

            return _analytics.GetExecutingJobs(authorizedJobGroups).ToList();
        }

        /// <summary>
        /// Get errored jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/erroredJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetErroredJobs(int count)
        {
            Logger.Debug("Entered AnalyticsController.GetErroredJobs().");
            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups();

            var erroredJobs = _analytics.GetErroredJobs(count, authorizedJobGroups);

            IList<FireInstance> erroredFireInstances = new List<FireInstance>();

            foreach (AuditLog erroredJob in erroredJobs)
            {
                var fi = Mapper.Map<FireInstance>(erroredJob);
                erroredFireInstances.Add(fi);
            }

            return erroredFireInstances;
        }

        /// <summary>
        /// Get executed jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/executedJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetExecutedJobs(int count)
        {
            Logger.Debug("Entered AnalyticsController.GetExecutedJobs().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            var executedJobs = _analytics.GetExecutedJobs(count, authorizedJobGroups);
            
            IList<FireInstance> erroredFireInstances = new List<FireInstance>();

            foreach (AuditLog erroredJob in executedJobs)
            {
                var fi = Mapper.Map<FireInstance>(erroredJob);
                erroredFireInstances.Add(fi);
            }

            return erroredFireInstances;
        }

        /// <summary>
        /// Get upcoming jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/upcomingJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetUpcomingJobs(int count)
        {
            Logger.Debug("Entered AnalyticsController.GetUpcomingJobs().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            var upcomingJobs = _analytics.GetUpcomingJobs(count, authorizedJobGroups).ToList();

            return upcomingJobs;
        }

        /// <summary>
        /// Get upcoming jobs between dates
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/upcomingJobsBetween")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetUpcomingJobsBetween(DateTime from, DateTime to)
        {
            Logger.Debug("Entered AnalyticsController.GetUpcomingJobsBetween().");
            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            var upcomingJobs = _analytics.GetUpcomingJobsBetween(from, to, authorizedJobGroups).ToList();

            return upcomingJobs;
        }

        /// <summary>
        /// Get job executions between dates
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobExecutionsBetween")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<FireInstance> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to)
        {
            Logger.Debug("Entered AnalyticsController.GetJobExecutionsBetween().");

            var authorizedJobGroups = _permissionsHelper.GetAuthorizedJobGroups().ToList();

            var executedJobs = _analytics.GetJobExecutionsBetween(id, from, to, authorizedJobGroups);

            IList<FireInstance> executedFireInstances = new List<FireInstance>();

            foreach (AuditLog executedJob in executedJobs)
            {
                var fi = Mapper.Map<FireInstance>(executedJob);
                executedFireInstances.Add(fi);
            }

            return executedFireInstances;
        }
    }
}
