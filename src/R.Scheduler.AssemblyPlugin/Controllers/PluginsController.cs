using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using Quartz;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public class PluginsController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;
        readonly IJobTypeManager _pluginManager;
        private const string JobType = "AssemblyPlugin";

        public PluginsController()
        {
            _pluginRepository = ObjectFactory.GetInstance<ICustomJobStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
            _pluginManager = ObjectFactory.GetInstance<IJobTypeManager>();
        }

        // GET api/values 
        [Route("api/plugins")]
        public IEnumerable<Plugin> Get()
        {
            Logger.Info("Entered PluginsController.Get().");

            IList<ICustomJob> registeredPlugins = _pluginRepository.GetRegisteredJobs(JobType);

            return
                registeredPlugins.Select(
                    registeredPlugin =>
                        new Plugin
                        {
                            Name = registeredPlugin.Name,
                            AssemblyPath = registeredPlugin.Params,
                            Id = registeredPlugin.Id
                        }).ToList();
        }

        /// <summary>
        /// Schedules a temporary job for an immediate execution
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins/execute")]
        public QueryResponse Execute([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Execute(). name = {0}", model);

            ICustomJob registeredPlugin = base.GetRegisteredCustomJob(model, JobType);

            var response = new QueryResponse {Valid = true};
            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", model);
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredPluginNotFound",
                        Type = "Sender",
                        Message = string.Format("Plugin {0} not found", model)
                    }
                };

                return response;
            }

            var dataMap = new Dictionary<string, object> {{"pluginPath", registeredPlugin.Params}};

            try
            {
                _schedulerCore.ExecuteJob(typeof (PluginRunner), dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorTriggeringPlugin",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="model">Pluin name</param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins/deschedule")]
        public QueryResponse Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Deschedule(). name = {0}", model);

            ICustomJob registeredPlugin = base.GetRegisteredCustomJob(model, JobType);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJobGroup(registeredPlugin.Id.ToString());
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingJobGroup",
                        Type = "Server",
                        Message = string.Format("Error:{0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // POST api/plugins 
        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public QueryResponse Post([FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _pluginManager.Register(model.Name, model.AssemblyPath);
            }
            catch (Exception ex)
            {
                string type = "Server";
                if (ex is FileNotFoundException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRegisteringPlugin",
                        Type = type,
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // PUT api/plugins/{id}
        [AcceptVerbs("PUT")]
        [Route("api/plugins/{id}")]
        public QueryResponse Put(string id, [FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Put(). name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _pluginRepository.UpdateName(new Guid(id), model.Name);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorUpdatingPlugin",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        // GET api/values 
        [Route("api/plugins/{id}")]
        public PluginDetails Get(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Get(). id = {0}", id);

            ICustomJob registeredPlugin = base.GetRegisteredCustomJob(id, JobType);

            // Still couldn't get, return null
            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", id);
                return null;
            }

            var retval = new PluginDetails
            {
                Name = registeredPlugin.Name,
                AssemblyPath = registeredPlugin.Params,
                TriggerDetails = new List<TriggerDetails>()
            };
            
            var quartzTriggers = _schedulerCore.GetTriggersOfJobGroup(registeredPlugin.Id.ToString());

            foreach (ITrigger quartzTrigger in quartzTriggers)
            {
                var triggerType = string.Empty;
                if (quartzTrigger is ICronTrigger)
                {
                    triggerType = "Cron";
                }
                if (quartzTrigger is ISimpleTrigger)
                {
                    triggerType = "Simple";
                }
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();
                retval.TriggerDetails.Add(new TriggerDetails
                {
                    Name = quartzTrigger.Key.Name,
                    Group = quartzTrigger.Key.Group,
                    JobName = quartzTrigger.JobKey.Name,
                    JobGroup = quartzTrigger.JobKey.Group,
                    Description = quartzTrigger.Description,
                    StartTimeUtc = quartzTrigger.StartTimeUtc.UtcDateTime,
                    EndTimeUtc = (quartzTrigger.EndTimeUtc.HasValue) ? quartzTrigger.EndTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc = (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue) ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    Type = triggerType
                });
            }

            return retval;
        }

        // DELETE api/plugins/id 
        [AcceptVerbs("DELETE")]
        [Route("api/plugins")]
        public QueryResponse Delete(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Delete(). id = {0}", id);

            var registeredPlugin = base.GetRegisteredCustomJob(id, JobType);

            var response = new QueryResponse { Valid = true };

            _schedulerCore.RemoveJobGroup(registeredPlugin.Id.ToString());

            int result = _pluginRepository.Remove(registeredPlugin.Id);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing from data store. Plugin {0} not found", id);

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredPluginNotFound",
                        Type = "Sender",
                        Message = string.Format("Plugin {0} not found", id)
                    }
                };
            }

            return response;
        }
    } 
}
