using System;
using System.Web.Http;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.PipesAndFilters.Controllers
{
    public abstract class BaseController : ApiController
    {
        private readonly ICustomJobStore _repository;

        protected BaseController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
        }

        protected ICustomJob GetRegisteredCustomJob(string id, string jobType)
        {
            ICustomJob registeredJob = null;

            // Try to get plugin by id
            Guid guidId;
            if (Guid.TryParse(id, out guidId))
            {
                registeredJob = _repository.GetRegisteredJob(guidId);
            }

            // Couldn't get it by id, try by name
            if (null == registeredJob)
            {
                registeredJob = _repository.GetRegisteredJob(id, jobType);
            }

            return registeredJob;
        }
    }
}
