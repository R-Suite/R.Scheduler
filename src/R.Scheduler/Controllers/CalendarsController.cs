using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.Calendars.Holiday.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class CalendarsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public CalendarsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all the calendar names
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/calendars")]
        public IEnumerable<string> Get()
        {
            Logger.Debug("Entered CalendarsController.Get().");

            return _schedulerCore.GetCalendarNames();
        }

        /// <summary>
        /// Delete calendar.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/calendars/{name}")]
        public QueryResponse Delete(string name)
        {
            Logger.DebugFormat("Entered CalendarsController.Delete(). name = {0}", name);

            var response = new QueryResponse { Valid = true };

            try
            {
                response.Valid = _schedulerCore.DeleteCalendar(name);
            }
            catch (Exception ex)
            {
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
                        Code = "ErrorDeletingCalendar",
                        Type = type,
                        Message = string.Format("Error deleting calendar {0}.", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Create new <see cref="HolidayCalendar"/> with optional exclusion dates
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/holidayCalendars")]
        public QueryResponse CreateHolidayCalendar([FromBody]HolidayCalendar model)
        {
            Logger.DebugFormat("Entered CalendarsController.Post(). Calendar Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                var id = _schedulerCore.AddHolidayCalendar(model.Name, model.Description, model.DatesExcluded);
                response.Id = id;
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
    }
}
