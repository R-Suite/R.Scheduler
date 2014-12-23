namespace R.Scheduler.Contracts.JobTypes.Ftp.Model
{
    public class FtpDownloadJob : BaseJob
    {
        public string FtpHost { get; set; }
        public string ServerPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LocalDirectoryPath { get; set; }
        public string RemotelDirectoryPath { get; set; }
        public string FileExtensions { get; set; }
        public string CutOffTimeSpan { get; set; }
    }
}
