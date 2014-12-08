using System;
using System.Collections.Generic;

namespace R.Scheduler.Contracts.Calendars.Cron.Model
{
    public class CronCalendar
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronExpression { get; set; }
    }
}
