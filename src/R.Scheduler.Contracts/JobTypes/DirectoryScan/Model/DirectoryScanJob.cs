using System;

namespace R.Scheduler.Contracts.JobTypes.DirectoryScan.Model
{
    public class DirectoryScanJob : BaseJob
    {
        public string DirectoryName { get; set; }
        public string CallbackUrl { get; set; }
        public long MinimumUpdateAge { get; set; }
        public DateTime LastModifiedTime { get; set; }
    }
}
