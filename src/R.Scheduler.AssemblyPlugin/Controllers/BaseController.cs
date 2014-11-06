using System;
using System.Web.Http;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public abstract class BaseController : ApiController
    {
        private readonly IPluginStore _pluginRepository;

        protected BaseController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
        }

        protected Plugin GetRegisteredPlugin(string id)
        {
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

            return registeredPlugin;
        }
    }
}
