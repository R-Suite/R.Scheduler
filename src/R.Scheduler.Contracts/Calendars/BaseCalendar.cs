using System;

namespace R.Scheduler.Contracts.Calendars
{
    public class BaseCalendar
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CalendarType { get; set; }
        public string SchedulerName { get; set; }
    }
}
