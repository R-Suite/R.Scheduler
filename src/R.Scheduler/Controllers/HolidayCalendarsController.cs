using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.Calendars.Holiday.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class HolidayCalendarsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public HolidayCalendarsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Create new HolidayCalendar with optional set of exclusion dates
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/holidayCalendars")]
        public QueryResponse Post([FromBody]HolidayCalendar model)
        {
            Logger.DebugFormat("Entered HolidayCalendarsController.Post(). Calendar Name = {0}", model.Name);

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

        /// <summary>
        /// Modify new HolidayCalendar
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/holidayCalendars/{id}")]
        public QueryResponse Put(Guid id, [FromBody]HolidayCalendar model)
        {
            Logger.DebugFormat("Entered HolidayCalendarsController.Put(). Calendar id = {0}", id);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.AmendHolidayCalendar(id, model.Description, model.DatesExcluded);
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
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/holidayCalendars/{id}")]
        public QueryResponse PostExclusionDate(Guid id, [FromBody]DateTime date)
        {
            Logger.DebugFormat("Entered HolidayCalendarsController.PostExclusionDate(). Calendar id = {0}, ExclusionDate = {1}", id, date);

            var response = new QueryResponse {Valid = true, Id = id};

            try
            {
                _schedulerCore.AddHolidayCalendarExclusionDates(id, new List<DateTime> { date });
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

        /// <summary>
        /// Get <see cref="HolidayCalendar"/>
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/holidayCalendars/{id}")]
        public HolidayCalendar Get(Guid id)
        {
            Logger.Debug("Entered HolidayCalendarsController.Get().");

            Quartz.Impl.Calendar.HolidayCalendar calendar;

            string calendarName;

            try
            {
                calendar = (Quartz.Impl.Calendar.HolidayCalendar) _schedulerCore.GetCalendar(id, out calendarName);
            }
            catch (Exception ex)
            {
                Logger.Debug(string.Format("Error getting JobDetail: {0}", ex.Message));
                return null;
            }

            return new HolidayCalendar
            {
                Id = id,
                Name = calendarName,
                DatesExcluded = calendar.ExcludedDates.ToList(),
                Description = calendar.Description
            };
        }
    }
}
