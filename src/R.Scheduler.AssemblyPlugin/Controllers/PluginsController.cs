using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public class PluginsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _repository;

        public PluginsController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
        }

        // GET api/values 
        [Route("api/plugins")]
        public IEnumerable<Plugin> Get()
        {
            Logger.Info("Entered PluginsController.Get().");

            IList<ICustomJob> registeredPlugins = _repository.GetRegisteredJobs(typeof(AssemblyPluginJob).Name);

            return registeredPlugins.Select(registeredPlugin =>
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

            return ExecuteCustomJob(model, "pluginPath", typeof(AssemblyPluginJob));
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="model">Plugin name</param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins/deschedule")]
        public QueryResponse Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Deschedule(). name = {0}", model);

            return DescheduleCustomJob(model, typeof(AssemblyPluginJob));
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public QueryResponse Post([FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). name = {0}", model.Name);

            return RegisterCustomJob(new CustomJob { Name = model.Name, Params = model.AssemblyPath, JobType = typeof(AssemblyPluginJob).Name });
        }

        [AcceptVerbs("PUT")]
        [Route("api/plugins/{id}")]
        public QueryResponse Put(string id, [FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Put(). name = {0}", model.Name);

            return UpdateCustomJob(id, new CustomJob { Name = model.Name });
        }

        [Route("api/plugins/{id}")]
        public PluginDetails Get(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Get(). id = {0}", id);

            ICustomJob registeredJob = base.GetRegisteredCustomJob(id, typeof(AssemblyPluginJob).Name);

            if (null == registeredJob)
            {
                Logger.ErrorFormat("Error getting registered plugin {0}", id);
                return null;
            }

            var retval = new PluginDetails
            {
                Name = registeredJob.Name,
                AssemblyPath = registeredJob.Params,
                TriggerDetails = new List<TriggerDetails>()
            };

            retval.TriggerDetails = GetCustomJobTriggerDetails(registeredJob);

            return retval;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/plugins")]
        public QueryResponse Delete(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Delete(). id = {0}", id);

            return DeleteCustomJob(id, typeof(AssemblyPluginJob));
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/{id}/simpleTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). Name = {0}", model.TriggerName);

            return CreateCustomJobSimpleTrigger(id, model, "pluginPath", typeof(AssemblyPluginJob));
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/{id}/cronTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobCronTrigger model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). Name = {0}", model.TriggerName);

            return CreateCustomJobCronTrigger(id, model, "pluginPath", typeof(AssemblyPluginJob));
        }
    } 
}
