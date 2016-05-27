using System;
using System.Configuration;
using System.Reflection;
using Common.Logging;
using Quartz;
using R.Scheduler.Core;
using R.Scheduler.Core.FeatureToggles;
using StructureMap;

namespace R.Scheduler.Ftp
{
    /// <summary>
    /// A job that implements a specific ftp file download scenario:
    /// Downloads all the files with specified extension, that are no older than a specified cut-off timespan, into a local directory.
    /// </summary>
    public class FtpDownloadJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The host name of the ftp server. REQUIRED.</summary>
        public const string FtpHost = "ftpHost";

        /// <summary> The port of the ftp server. Optional.</summary>
        public const string ServerPort = "serverPort";

        /// <summary> Username for authenticated session.</summary>
        public const string UserName = "userName";

        /// <summary> Password for authenticated session. Optional.</summary>
        public const string Password =  "password";

        /// <summary> Local directory path. REQUIRED.</summary>
        public const string LocalDirectoryPath = "localDirectoryPath";

        /// <summary> Remote directory path. Optional.</summary>
        public const string RemoteDirectoryPath = "remoteDirectoryPath";

        /// <summary> Cut-off time span. Optional.</summary>
        public const string CutOff = "cutOffTimeSpan";

        /// <summary> Single or comma-separated list of file extensions. REQUIRED.</summary>
        public const string FileExtensions = "fileExtensions";

        /// <summary>
        /// Ctor used by Scheduler engine
        /// </summary>
        public FtpDownloadJob()
        {
            Logger.Debug("Entering FtpDownloadJob.ctor().");
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;
            var jobName = context.JobDetail.Key.Name;

            string ftpHost = GetRequiredParameter(data, FtpHost, jobName);
            string serverPort = GetOptionalParameter(data, ServerPort);
            string userName = GetOptionalParameter(data, UserName);
            string password = GetOptionalParameter(data, Password);
            string localDirectoryPath = GetRequiredParameter(data, LocalDirectoryPath, jobName);
            string remoteDirectoryPath = GetOptionalParameter(data, RemoteDirectoryPath);
            string cutOff = GetOptionalParameter(data, CutOff);
            string fileExtensions = GetRequiredParameter(data, FileExtensions, jobName);

            // Set defaults
            int port = (!string.IsNullOrEmpty(serverPort) ? Int32.Parse(serverPort) : 21);
            cutOff = (!string.IsNullOrEmpty(cutOff) ? cutOff : "1.00:00:00"); // 1 day

            // Validate cutOffTimeSpan format
            TimeSpan cutOffTimeSpan;
            if (!TimeSpan.TryParse(cutOff, out cutOffTimeSpan))
            {
                var err = string.Format("Invalid cutOffTimeSpan format [{0}] specified.", cutOff);
                Logger.ErrorFormat("Error in FtpDownloadJob ({0}): {1}", jobName, err);
                throw new JobExecutionException(err);
            }

            try
            {
                if (new EncryptionFeatureToggle().FeatureEnabled)
                {
                    userName = AESGCM.SimpleDecrypt(userName, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                    password = AESGCM.SimpleDecrypt(password, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigurationError executing SecureWebRequestJob job.", ex);
            }

            // Get files
            try
            {
                using (var ftpLibrary = ObjectFactory.GetInstance<IFtpLibrary>())
                {
                    ftpLibrary.Connect(ftpHost, port, userName, password);
                    ftpLibrary.GetFiles(remoteDirectoryPath, localDirectoryPath, fileExtensions, cutOffTimeSpan);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error in FtpDownloadJob ({0}):", jobName), ex);
                throw new JobExecutionException(ex.Message, ex, false);
            }
        }

        protected virtual string GetOptionalParameter(JobDataMap data, string propertyName)
        {
            string value = data.GetString(propertyName);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value;
        }

        protected virtual string GetRequiredParameter(JobDataMap data, string propertyName, string jobName)
        {
            string value = data.GetString(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                Logger.ErrorFormat("Error in FtpDownloadJob ({0}): {1} not specified.", jobName, propertyName);
                throw new JobExecutionException(string.Format("{0} not specified.", propertyName));
            }
            return value;
        }
    }
}
