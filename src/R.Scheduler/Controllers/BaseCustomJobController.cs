using System;
using System.Collections.Generic;
using System.Web.Http;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public abstract class BaseCustomJobController : ApiController
    {
        private readonly ICustomJobStore _repository;
        readonly ISchedulerCore _schedulerCore;

        protected BaseCustomJobController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
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

        protected QueryResponse ExecuteCustomJob(string model, string jobType, string dataMapParamKey, Type jobRunnerType)
        {
            ICustomJob registeredJob = GetRegisteredCustomJob(model, jobType);

            var response = new QueryResponse { Valid = true };
            if (null == registeredJob)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("{0} not found", model)
                    }
                };

                return response;
            }

            var dataMap = new Dictionary<string, object> { { dataMapParamKey, registeredJob.Params } };

            try
            {
                _schedulerCore.ExecuteJob(jobRunnerType, dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorTriggeringCustomJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }
            return response;
        }

    }
}
