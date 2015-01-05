using System;
using R.Scheduler.Ftp;

namespace R.Scheduler.FakeFtpLib
{
    public class FakeFtpLibrary : IFtpLibrary
    {
        public void Dispose()
        {
        }

        public void Connect(string host, int serverPort, string userName, string password)
        {
        }

        public void GetFiles(string remotePath, string localDir, string fileExtension, TimeSpan cutOff)
        {
        }
    }
}
