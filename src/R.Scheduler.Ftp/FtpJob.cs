using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Common.Logging;
using Quartz;
using StructureMap;

namespace R.Scheduler.Ftp
{
    public class FtpJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Container _container;

        /// <summary>
        /// Ctor used by Scheduler engine
        /// </summary>
        public FtpJob()
        {
            Logger.Info("Entering FtpJob.ctor().");
            _container = new Container(new SmRegistry());
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;

            string ftpHost = GetRequiredParameter(data, "ftpHost");
            string serverPort = GetOptionalParameter(data, "serverPort");
            string userName = GetOptionalParameter(data, "userName");
            string password = GetOptionalParameter(data, "password");

            string action = GetRequiredParameter(data, "action"); // PUT/GET
            string localDirectoryPath = GetRequiredParameter(data, "localDirectoryPath");
            string remoteDirectoryPath = GetOptionalParameter(data, "remoteDirectoryPath");
            string fileName = GetOptionalParameter(data, "fileName");

            string cutOff = GetOptionalParameter(data, "cutOffTimeSpan");
            string fileExtension = GetOptionalParameter(data, "fileExtension");

            if (action.ToLower() != "get" && action.ToLower() != "put")
            {
                throw new ArgumentException(string.Format("Invalid action {0} specified. Only GET/PUT values are allowed", action));
            }

            int port = (!string.IsNullOrEmpty(serverPort) ? Int32.Parse(serverPort) : 21);

            var ftpLibrary = _container.GetInstance<IFtpLibrary>();
            ftpLibrary.Connect(ftpHost, port, userName, password, remoteDirectoryPath);

            //ftpLibrary.GetFiles(cutOff, localDirectoryPath, fileExtension);

            //ftpLibrary.GetFile(fileName, Path.Combine(localDirectoryPath, fileName));
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

        protected virtual string GetRequiredParameter(JobDataMap data, string propertyName)
        {
            string value = data.GetString(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(propertyName + " not specified.");
            }
            return value;
        }
    }
}
