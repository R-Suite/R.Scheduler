using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.Calendars.Holiday.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Controllers
{
    public class HolidayCalendarsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public HolidayCalendarsController(ISchedulerCore schedulerCore)
        {
            _schedulerCore = schedulerCore;
        }

        /// <summary>
        /// Create new HolidayCalendar with optional set of exclusion dates
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/calendars/holiday")]
        public QueryResponse Post([FromBody]HolidayCalendar model)
        {
            Logger.InfoFormat("Entered HolidayCalendarsController.Post(). Calendar Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AddHolidayCalendar(model.Name, model.Description, model.DatesExcluded);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingHolidayCalendar",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Modify new HolidayCalendar
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/calendars/holiday/{name}")]
        public QueryResponse Put(string name, [FromBody]HolidayCalendar model)
        {
            Logger.InfoFormat("Entered HolidayCalendarsController.Put(). Calendar Name = {0}", name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AddHolidayCalendar(name, model.Description, model.DatesExcluded);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorEditingHolidayCalendar",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Update HolidayCalendar with exclusion date
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/calendars/holiday/{name}")]
        public QueryResponse PostExclusionDate(string name, [FromBody]DateTime date)
        {
            Logger.InfoFormat("Entered HolidayCalendarsController.PostExclusionDate(). Calendar Name = {0}, ExclusionDate = {1}", name, date);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AddHolidayCalendarExclusionDates(name, new List<DateTime> { date });
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorAddingExclusionDates",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
