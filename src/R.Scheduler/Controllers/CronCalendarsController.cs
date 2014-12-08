using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.Calendars.Cron.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class CronCalendarsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public CronCalendarsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Create new CronCalendar with optional set of exclusion dates
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/cronCalendars")]
        public QueryResponse Post([FromBody]CronCalendar model)
        {
            Logger.InfoFormat("Entered CronCalendarsController.Post(). Calendar Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AddCronCalendar(model.Name, model.Description, model.CronExpression);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingCronCalendar",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Modify CronCalendar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/cronCalendars/{name}")]
        public QueryResponse Put(string name, [FromBody]CronCalendar model)
        {
            Logger.InfoFormat("Entered CronCalendarsController.Put(). Calendar Name = {0}", name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AddCronCalendar(name, model.Description, model.CronExpression);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorEditingCronCalendar",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
