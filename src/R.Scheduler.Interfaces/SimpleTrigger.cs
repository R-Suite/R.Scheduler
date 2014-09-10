using System;

namespace R.Scheduler.Interfaces
{
    public class SimpleTrigger : BaseTrigger
    {
        public int RepeatCount { get; set; }
        public TimeSpan RepeatInterval { get; set; }
    }
}
