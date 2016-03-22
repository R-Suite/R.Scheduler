using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using R.Scheduler.Core;

namespace R.Scheduler.Controllers
{
    public class CronExpressionController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Get specified number of fire times after a specified date.
        /// Date defaults to the curent datetime if not specified.
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/cronExpressions/fireTimesAfter")]
        public List<DateTime> GetFutureDateTimes(string cronExpression, string dateTimeAfter = null, int count = 100)
        {
            Logger.Debug("Entered CronExpressionController.GetFutureDateTimes()");

            DateTime dta = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dateTimeAfter))
            {
                dta = DateTime.Parse(dateTimeAfter);
            }

            var cronExpressionEx = new CronExpressionEx(cronExpression);

            return cronExpressionEx.GetFutureFireDateTimesUtcAfter(dta, count);
        }

        /// <summary>
        /// Get cron string expression summary.
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/cronExpressions/expressionSummary")]
        public string GetExpressionSummary(string cronExpression)
        {
            Logger.Debug("Entered CronExpressionController.GetExpressionSummary()");

            var cronExpressionEx = new CronExpressionEx(cronExpression);

            return cronExpressionEx.GetExpressionSummary();
        }
    }
}
