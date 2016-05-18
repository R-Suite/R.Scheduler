using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Contracts.Calendars;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
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
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IEnumerable<BaseCalendar> Get()
        {
            Logger.Debug("Entered CalendarsController.Get().");

            var quartzBaseCalendars =  _schedulerCore.GetCalendars();

            return quartzBaseCalendars.Select(i =>
                                                    new BaseCalendar
                                                    {
                                                        Id = i.Value.Value,
                                                        Name = i.Value.Key,
                                                        CalendarType = i.Key.GetType().Name,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        Description = i.Key.Description
                                                    }).ToList();

        }

        /// <summary>
        /// Delete calendar.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/calendars/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Delete.Roles", AppSettingUsers = "Delete.Users")]
        public QueryResponse Delete(Guid id)
        {
            Logger.DebugFormat("Entered CalendarsController.Delete(). id = {0}", id);

            var response = new QueryResponse { Valid = true };

            try
            {
                response.Valid = _schedulerCore.DeleteCalendar(id);
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
    }
}
