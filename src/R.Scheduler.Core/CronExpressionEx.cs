using System;
using System.Collections.Generic;
using Quartz;
using StructureMap;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Extension of <see cref="CronExpression"/> 
    /// </summary>
    public class CronExpressionEx : CronExpression
    {
        private readonly IScheduler _scheduler;

        public CronExpressionEx(string cronExpression)
            : base(cronExpression)
        {
            _scheduler = ObjectFactory.GetInstance<IScheduler>();
        }

        public CronExpressionEx(string cronExpression, IScheduler scheduler)
            : base(cronExpression)
        {
            _scheduler = scheduler;
        }

        /// <summary>
        /// Get future date/times which satisfies the cron expression. 
        /// Optionally exclude dates of a provided calendar.
        /// </summary>
        /// <param name="dateTimeAfter"></param>
        /// <param name="count"></param>
        /// <param name="calendarName"></param>
        /// <returns></returns>
        public List<DateTime> GetFutureFireDateTimesUtcAfter(DateTime dateTimeAfter, int count, string calendarName = null)
        {
            var retval = new List<DateTime>();

            ICalendar calendar = null;
            if (!string.IsNullOrEmpty(calendarName))
            {
                calendar = _scheduler.GetCalendar(calendarName);
            }

            int runningCount = 0;
            int controlCount = 0;
            DateTime runningDateTime = dateTimeAfter;
            while (runningCount < count && controlCount < (count * 100)) // ensure we don't get into endless loop
            {
                controlCount++;
                var res = base.GetNextValidTimeAfter(runningDateTime);
                if (res.HasValue)
                {
                    runningDateTime = res.Value.UtcDateTime;

                    if (null != calendar && !calendar.IsTimeIncluded(res.Value))
                    {
                        continue;
                    }

                    retval.Add(res.Value.UtcDateTime);
                    runningCount++;
                }
            }

            return retval;
        }
    }
}
