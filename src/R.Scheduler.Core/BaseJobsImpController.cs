using System;
using System.Collections.Generic;
using System.Web.Http;
using R.Scheduler.Contracts.JobTypes;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    public abstract class BaseJobsImpController : ApiController
    {
        readonly ISchedulerCore _schedulerCore;

        protected BaseJobsImpController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        protected QueryResponse CreateJob(BaseJob model, Type jobType, Dictionary<string, object> dataMap)
        {
            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.CreateJob(model.JobName, model.JobGroup, jobType, dataMap);
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
