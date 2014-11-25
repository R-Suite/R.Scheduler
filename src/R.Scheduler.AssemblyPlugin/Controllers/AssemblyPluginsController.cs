using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public class AssemblyPluginsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected AssemblyPluginsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the jobs of type <see cref="AssemblyPluginJob"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/plugins")]
        public IEnumerable<PluginJob> Get()
        {
            Logger.Info("Entered AssemblyPluginsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof(AssemblyPluginJob));

            return jobDetails.Select(jobDetail =>
                                                    new PluginJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        AssemblyPath = jobDetail.JobDataMap.GetString("pluginPath"),
                                                    }).ToList();

        }

        /// <summary>
        /// Create new AssemblyPluginJob without any triggers
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public QueryResponse Post([FromBody]PluginJob model)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Post(). Job Name = {0}", model.JobName);

            var response = new QueryResponse { Valid = true };

            var dataMap = new Dictionary<string, object>
            {
                {"pluginPath", model.AssemblyPath},
            };

            try
            {
                _schedulerCore.CreateJob(model.JobName, model.JobGroup, typeof(AssemblyPluginJob), dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
