using System;
using System.Collections.Generic;
using System.Web.Http;
using R.Scheduler.Contracts.JobTypes;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    /// <summary>
    /// A base class that provides common functionality for job controllers
    /// </summary>
    public abstract class BaseJobsImpController : ApiController
    {
        readonly ISchedulerCore _schedulerCore;

        protected BaseJobsImpController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        protected QueryResponse CreateJob(BaseJob model, Type jobType, Dictionary<string, object> dataMap, string description = null)
        {
            var response = new QueryResponse { Valid = true };

            try
            {
                var id = _schedulerCore.CreateJob(model.JobName, model.JobGroup, jobType, dataMap, description, model.Id == Guid.Empty ? (Guid?)null : model.Id);
                response.Id = id;
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
