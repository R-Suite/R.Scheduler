using System;

namespace R.Scheduler.AssemblyPlugin.Contracts.DataContracts
{
    public class TriggerDetails
    {
        public string Description { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime? NextFireTimeUtc { get; set; }
        public DateTime? PreviousFireTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public DateTime? FinalFireTimeUtc { get; set; }
    }
}
