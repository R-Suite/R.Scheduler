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
            Logger.DebugFormat("Entered CronCalendarsController.Post(). Calendar Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                var id = _schedulerCore.AddCronCalendar(model.Name, model.Description, model.CronExpression);
                response.Id = id;
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
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/cronCalendars/{id}")]
        public QueryResponse Put(Guid id, [FromBody]CronCalendar model)
        {
            Logger.DebugFormat("Entered CronCalendarsController.Put(). Calendar id = {0}", id);

            var response = new QueryResponse { Valid = true, Id = id};

            try
            {
                _schedulerCore.AmendCronCalendar(id, model.Description, model.CronExpression);
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
