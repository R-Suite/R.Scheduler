using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class PluginsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;

        public PluginsController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
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

            _schedulerCore.ExecutePlugin(model);
        }

        // POST api/plugins/execute 
        [AcceptVerbs("POST")]
        [Route("api/plugins/deschedule")]
        public void Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered PluginsController.Deschedule(). name = {0}", model);

            _schedulerCore.DescheduleJobGroup(model);
        }

        // POST api/plugins 
        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public void Post([FromBody]Plugin model)
        {
            Logger.InfoFormat("Entered PluginsController.Post(). name = {0}", model.Name);

            _schedulerCore.RegisterPlugin(model.Name, model.AssemblyPath);
        }
        
        // GET api/plugins/5 
        public string Get(int id)
        {
            return "value";
        }

        // PUT api/plugins/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/plugins/id 
        [AcceptVerbs("DELETE")]
        [Route("api/plugins")]
        public void Delete(string id)
        {
            Logger.InfoFormat("Entered PluginsController.Delete(). id = {0}", id);

            _schedulerCore.RemovePlugin(id);
        }
    } 
}
