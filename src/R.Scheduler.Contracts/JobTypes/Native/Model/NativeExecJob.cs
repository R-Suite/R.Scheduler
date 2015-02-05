namespace R.Scheduler.Contracts.JobTypes.Native.Model
{
    public class NativeExecJob : BaseJob
    {
        public string Command { get; set; }
        public string Parameters { get; set; }
        public bool WaitForProcess { get; set; }
        public bool ConsumeStreams { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
