using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
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
        /// Delete calendar.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/calendars/{name}")]
        public QueryResponse Delete(string name)
        {
            Logger.InfoFormat("Entered CalendarsController.Delete(). name = {0}", name);

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
    }
}
