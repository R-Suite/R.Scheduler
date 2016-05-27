using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace R.Scheduler.Sql.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class SqlJobsController : BaseJobsImpController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected SqlJobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="SqlJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/sqlJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<Contracts.JobTypes.Sql.Model.SqlJob> Get()
        {
            Logger.Debug("Entered SqlJobsController.Get().");

            var jobDetailsMap = _schedulerCore.GetJobDetails(typeof(SqlJob));

            var retval = new List<Contracts.JobTypes.Sql.Model.SqlJob>();

            foreach (var mapItem in jobDetailsMap)
            {
                string connectionString = mapItem.Key.JobDataMap.GetString("connectionString");

                try
                {
                    if (new EncryptionFeatureToggle().FeatureEnabled)
                    {
                        connectionString = AESGCM.SimpleDecrypt(connectionString, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ConfigurationError getting SqlJob jobs.", ex);
                }

                retval.Add(new Contracts.JobTypes.Sql.Model.SqlJob
                {
                    Id = mapItem.Value,
                    JobName = mapItem.Key.Key.Name,
                    JobGroup = mapItem.Key.Key.Group,
                    SchedulerName = _schedulerCore.SchedulerName,
                    ConnectionString = connectionString,
                    CommandClass = mapItem.Key.JobDataMap.GetString("commandClass"),
                    ConnectionClass = mapItem.Key.JobDataMap.GetString("connectionClass"),
                    CommandStyle = mapItem.Key.JobDataMap.GetString("commandStyle"),
                    ProviderAssemblyName = mapItem.Key.JobDataMap.GetString("providerAssemblyName"),
                    NonQueryCommand = mapItem.Key.JobDataMap.GetString("nonQueryCommand"),
                    DataAdapterClass = mapItem.Key.JobDataMap.GetString("dataAdapterClass")
                });
            }

            return retval;
        }

        /// <summary>
        /// Get job details of <see cref="SqlJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/sqlJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public Contracts.JobTypes.Sql.Model.SqlJob Get(Guid id)
        {
            Logger.Debug("Entered SqlJobsController.Get().");

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

            string connectionString = jobDetail.JobDataMap.GetString("connectionString");

            try
            {
                if (new EncryptionFeatureToggle().FeatureEnabled)
                {
                    connectionString = AESGCM.SimpleDecrypt(connectionString, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigurationError getting SqlJob job.", ex);
            }

            return new Contracts.JobTypes.Sql.Model.SqlJob
            {
                Id = id,
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                SchedulerName = _schedulerCore.SchedulerName,
                ConnectionString = connectionString,
                CommandClass = jobDetail.JobDataMap.GetString("commandClass"),
                ConnectionClass = jobDetail.JobDataMap.GetString("connectionClass"),
                CommandStyle = jobDetail.JobDataMap.GetString("commandStyle"),
                ProviderAssemblyName = jobDetail.JobDataMap.GetString("providerAssemblyName"),
                NonQueryCommand = jobDetail.JobDataMap.GetString("nonQueryCommand"),
                DataAdapterClass = jobDetail.JobDataMap.GetString("dataAdapterClass"),
                Description = jobDetail.Description
            };
        }

        /// <summary>
        /// Create new <see cref="SqlJob"/> without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/sqlJobs")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]Contracts.JobTypes.Sql.Model.SqlJob model)
        {
            Logger.DebugFormat("Entered SqlJobsController.Post(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        /// <summary>
        /// Update <see cref="SqlJob"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/sqlJobs/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse Put([FromBody]Contracts.JobTypes.Sql.Model.SqlJob model)
        {
            Logger.DebugFormat("Entered SqlJobsController.Put(). Job Name = {0}", model.JobName);

            return CreateJob(model);
        }

        private QueryResponse CreateJob(Contracts.JobTypes.Sql.Model.SqlJob model)
        {
            string connectionString = model.ConnectionString;

            try
            {
                if (new EncryptionFeatureToggle().FeatureEnabled)
                {
                    connectionString = AESGCM.SimpleEncrypt(connectionString, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigurationError creating SqlJob job.", ex);
            }

            var dataMap = new Dictionary<string, object>
            {
                {"connectionString", connectionString},
                {"commandClass", model.CommandClass},
                {"connectionClass", model.ConnectionClass},
                {"commandStyle", model.CommandStyle},
                {"providerAssemblyName", model.ProviderAssemblyName},
                {"nonQueryCommand", model.NonQueryCommand},
                {"dataAdapterClass", model.DataAdapterClass}
            };

            return base.CreateJob(model, typeof (SqlJob), dataMap, model.Description);
        }
    }
}
