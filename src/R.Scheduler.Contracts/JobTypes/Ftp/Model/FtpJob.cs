namespace R.Scheduler.Contracts.JobTypes.Ftp.Model
{
    public class FtpJob : BaseJob
    {
        public string Action { get; set; }
        public string ServerPort { get; set; }
        public string FtpHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LocalDirectoryPath { get; set; }
        public string RemotelDirectoryPath { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string CutOffTimeSpan { get; set; }
    }
}
