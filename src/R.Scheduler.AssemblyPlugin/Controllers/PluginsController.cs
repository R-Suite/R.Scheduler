using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using Quartz;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public class PluginsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;
        readonly IJobTypeManager _pluginManager;

        public PluginsController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
            _pluginManager = ObjectFactory.GetInstance<IJobTypeManager>();
        }

        // GET api/values 
        [Route("api/plugins")]
        public IEnumerable<Plugin> Get()
        {
            Logger.Info("Entered PluginsController.Get().");

            var registeredPlugins = _pluginRepository.GetRegisteredPlugins();

            return registeredPlugins;
        }

        // POST api/plugins/execute 
        [AcceptVerbs("POST")]
        [Route("api/plugins/execute")]
        public void Execute([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Execute(). name = {0}", model);

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(model);

            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", model);
                return;
            }

            var dataMap = new Dictionary<string, object> {{"pluginPath", registeredPlugin.AssemblyPath}};

            _schedulerCore.ExecuteJob(typeof(PluginRunner), dataMap);
        }

        // POST api/plugins/execute 
        [AcceptVerbs("POST")]
        [Route("api/plugins/deschedule")]
        public void Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Deschedule(). name = {0}", model);

            _schedulerCore.RemoveJobGroup(model);
        }

        // POST api/plugins 
        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public void Post([FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). name = {0}", model.Name);

            _pluginManager.Register(model.Name, model.AssemblyPath);
        }

        // PUT api/plugins/{id}
        [AcceptVerbs("PUT")]
        [Route("api/plugins/{id}")]
        public void Put(string id, [FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Put(). name = {0}", model.Name);

            _pluginRepository.UpdatePluginName(new Guid(id), model.Name);
        }

        // GET api/values 
        [Route("api/plugins/{id}")]
        public PluginDetails Get(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Get(). id = {0}", id);

            Plugin registeredPlugin = null;

            // Try to get plugin by id
            Guid guidId;
            if (Guid.TryParse(id, out guidId))
            {
                registeredPlugin = _pluginRepository.GetRegisteredPlugin(guidId);
            }

            // Couldn't get it by id, try by name
            if (null == registeredPlugin)
            {
                registeredPlugin = _pluginRepository.GetRegisteredPlugin(id);
            }

            // Still couldn't get, return null
            if (null == registeredPlugin)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", id);
                return null;
            }

            var retval = new PluginDetails
            {
                Name = registeredPlugin.Name,
                AssemblyPath = registeredPlugin.AssemblyPath,
                TriggerDetails = new List<TriggerDetails>()
            };
            
            var quartzTriggers = _schedulerCore.GetTriggersOfJobGroup(registeredPlugin.Name);

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
        public void Delete(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Delete(). id = {0}", id);

            _schedulerCore.RemoveJobGroup(id);

            int result = _pluginRepository.RemovePlugin(id);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing from data store. Plugin {0} not found", id);
            }
        }
    } 
}
