using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R.Scheduler.Contracts.Model
{
    public class FireInstance
    {
        public DateTime TimeStamp { get; set; }
        public string Action { get; set; }
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string JobType { get; set; }
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public DateTimeOffset? FireTimeUtc { get; set; }
        public DateTimeOffset? ScheduledFireTimeUtc { get; set; }
        public TimeSpan JobRunTime { get; set; }
        public string Params { get; set; }
        public int RefireCount { get; set; }
        public bool Recovering { get; set; }
        public string Result { get; set; }
        public string ExecutionException { get; set; }
    }
}
