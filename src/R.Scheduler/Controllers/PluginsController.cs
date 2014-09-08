using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Contracts.Interfaces;
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
        public void Execute([FromBody]string name)
        {
            Logger.InfoFormat("Entered PluginsController.Execute(). name = {0}", name);

            _schedulerCore.ExecutePlugin(name);
        }

        // GET api/plugins/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/plugins 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/plugins/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/plugins/5 
        public void Delete(int id)
        {
        }
    } 
}
