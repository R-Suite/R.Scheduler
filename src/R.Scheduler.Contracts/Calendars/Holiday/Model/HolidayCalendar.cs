using System;
using System.Collections.Generic;

namespace R.Scheduler.Contracts.Calendars.Holiday.Model
{
    public class HolidayCalendar
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<DateTime> DatesExcluded { get; set; }
    }
}
