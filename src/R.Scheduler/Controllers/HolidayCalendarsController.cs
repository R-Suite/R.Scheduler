using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.Calendars.Holiday.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
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
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
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
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
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
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/holidayCalendars/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse PostExclusionDate(Guid id, [FromBody]AddExclusionDatesRequest model)
        {
            Logger.DebugFormat("Entered HolidayCalendarsController.PostExclusionDate(). Calendar id = {0}, ExclusionDate = {1}", id, model);

            var response = new QueryResponse {Valid = true, Id = id};

            try
            {
                _schedulerCore.AddHolidayCalendarExclusionDates(id, model.ExclusionDates);
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
        /// Delete exclusion date from specified HolidayCalendar
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/holidayCalendars/{id}/exclusionDate")]
        [SchedulerAuthorize(AppSettingRoles = "Delete.Roles", AppSettingUsers = "Delete.Users")]
        public QueryResponse RemoveExclusionDate(Guid id, [FromBody]RemoveExclusionDatesRequest model)
        {
            Logger.DebugFormat("Entered HolidayCalendarsController.RemoveExclusionDate(). Calendar id = {0}, ExclusionDate = {1}", id, model);

            var response = new QueryResponse { Valid = true, Id = id, Errors = new List<Error>() };

            // Get saved calendar
            Quartz.Impl.Calendar.HolidayCalendar calendar;
            try
            {
                string calendarName;
                calendar = (Quartz.Impl.Calendar.HolidayCalendar)_schedulerCore.GetCalendar(id, out calendarName);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error getting HolidayCalendar: {0}", ex.Message), ex);

                response.Valid = false;
                response.Errors.Add(new Error
                {
                    Code = "ErrorGettingCalendar",
                    Type = "Server",
                    Message = string.Format("Error: {0}", ex.Message)
                });

                return response;
            }

            // Remove specified exclusion dates
            var newExclusionDates = new List<DateTime>();

            foreach (var excludedDate in calendar.ExcludedDates)
            {
                if (!model.ExclusionDates.Contains(excludedDate))
                {
                    newExclusionDates.Add(excludedDate);
                }
            }

            // Amend calendar
            try
            {
                _schedulerCore.AmendHolidayCalendar(id, calendar.Description, newExclusionDates);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorAmendingHolidayCalendar",
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
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
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
                Logger.Error(string.Format("Error getting HolidayCalendar: {0}", ex.Message), ex);
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
