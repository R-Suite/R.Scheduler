using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class JobsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public JobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("DELETE")]
        [Route("api/jobs")]
        public QueryResponse Delete(string jobName)
        {
            Logger.InfoFormat("Entered JobsController.Delete(). jobName = {0}", jobName);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJob(jobName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing job  {0}. {1}", jobName, ex.Message);

                string type = "Server";
                if (ex is ArgumentException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingJob",
                        Type = type,
                        Message = string.Format("Error removing job {0}.", jobName)
                    }
                };
            }

            return response;
        }
    }
}
