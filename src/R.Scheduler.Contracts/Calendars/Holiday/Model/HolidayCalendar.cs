using System;
using System.Collections.Generic;

namespace R.Scheduler.Contracts.Calendars.Holiday.Model
{
    public class HolidayCalendar : BaseCalendar
    {
        public IList<DateTime> DatesExcluded { get; set; }
    }
}
