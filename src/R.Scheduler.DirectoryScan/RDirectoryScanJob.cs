using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Common.Logging;
using Quartz;
using Quartz.Job;

namespace R.Scheduler.DirectoryScan
{
    public class RDirectoryScanListener : IDirectoryScanListener
    {
        private readonly string _callbackUrl;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RDirectoryScanListener(string callbackUrl)
        {
            _callbackUrl = callbackUrl;
        }

        public void FilesUpdatedOrAdded(IEnumerable<FileInfo> updatedFiles)
        {
            var fileInfos = updatedFiles as IList<FileInfo> ?? updatedFiles.ToList();
            Logger.InfoFormat("Found {0} updated files", fileInfos.Count);

            // execute _callbackUrl
            var request = WebRequest.Create(_callbackUrl);
            request.Method = "POST";

            var sbFiles = new StringBuilder();
            foreach (var updatedFileInfo in fileInfos)
            {
                sbFiles.AppendLine(updatedFileInfo.FullName);
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(sbFiles.ToString());
            request.ContentType = "text/plain";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // Get the response.
            WebResponse response = request.GetResponse();
            Logger.InfoFormat("RDirectoryScanListener server response status: {0}", ((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Logger.InfoFormat("RDirectoryScanListener server response content: {0}", responseFromServer);

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    }

    public class RDirectoryScanJob : DirectoryScanJob, IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The callback url where list of updated file path are posted. REQUIRED.</summary>
        public const string CallbackUrl = "CALLBACK_URL";

        public new void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;

            string callbackUrl = GetRequiredParameter(data, CallbackUrl);

            var listener = new RDirectoryScanListener(callbackUrl);

            context.Scheduler.Context.Add("RDirectoryScanListener", listener);

            Logger.Debug("Adding RDirectoryScanListener to context.");

            base.Execute(context);
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
