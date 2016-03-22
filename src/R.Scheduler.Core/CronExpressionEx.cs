using System;
using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Core
{
    public class CronExpressionEx : CronExpression
    {
        public CronExpressionEx(string cronExpression)
            : base(cronExpression)
        {
        }

        public List<DateTime> GetFutureFireDateTimesUtcAfter(DateTime dateTimeAfter, int count)
        {
            var retval = new List<DateTime>();

            int runningCount = 0;
            DateTime runningDateTime = dateTimeAfter;
            while (runningCount < count)
            {
                runningCount++;
                var res = base.GetNextValidTimeAfter(runningDateTime);
                if (res.HasValue)
                {
                    retval.Add(res.Value.UtcDateTime);
                    runningDateTime = res.Value.UtcDateTime;
                }
            }

            return retval;
        }
    }
}
