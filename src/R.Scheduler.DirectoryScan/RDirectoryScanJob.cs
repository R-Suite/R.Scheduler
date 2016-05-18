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
    /// <summary>
    /// Implementation of <see cref="IDirectoryScanListener"/> that defines a call back method
    /// for when one or more files  in a directory have been created or updated.
    /// </summary>
    public class RDirectoryScanListener : IDirectoryScanListener
    {
        private readonly string _callbackUrl;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RDirectoryScanListener(string callbackUrl)
        {
            _callbackUrl = callbackUrl;
        }

        /// <summary>
        /// Creates a web request that posts updated or created files to a specified url.
        /// Let exceptions to bubble up to <see cref="DirectoryScanJob"/> 
        /// so that the job picks up the same files next time.
        /// </summary>
        /// <param name="updatedFiles"></param>
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
                Logger.DebugFormat("updatedFileInfo.FullName: {0}", updatedFileInfo.FullName);
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
            Logger.InfoFormat("RDirectoryScanListener server response status: {0}, {1}", ((HttpWebResponse)response).StatusCode, ((HttpWebResponse)response).StatusDescription);

            // Clean up the streams.
            dataStream.Close();
            response.Close();
        }
    }

    /// <summary>
    /// Simple wrapper for <see cref="DirectoryScanJob"/> that allows
    /// </summary>
    public class RDirectoryScanJob : DirectoryScanJob, IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The callback url where list of updated file path are posted. REQUIRED.</summary>
        public const string CallbackUrl = "CALLBACK_URL";

        /// <summary>
        /// Registers new instance of <see cref="IDirectoryScanListener"/> with the scheduler context,
        /// executes the job, and removes the instance from the scheduler context
        /// </summary>
        /// <param name="context"></param>
        public new void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;
            var jobName = context.JobDetail.Key.Name;

            string callbackUrl = GetRequiredParameter(data, CallbackUrl, jobName);

            string listenerName = Guid.NewGuid().ToString();
            var listener = new RDirectoryScanListener(callbackUrl);

            Logger.DebugFormat("Adding {0} to context.", listenerName);

            data.Put("DIRECTORY_SCAN_LISTENER_NAME", listenerName);
            context.Scheduler.Context.Add(listenerName, listener);

            Logger.InfoFormat("Executing ({0})", jobName);
            base.Execute(context);

            context.Scheduler.Context.Remove(listenerName);
        }

        protected virtual string GetRequiredParameter(JobDataMap data, string propertyName, string jobName)
        {
            string value = data.GetString(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                Logger.ErrorFormat("Error in RDirectoryScanJob ({0}): {1} not specified.", jobName, propertyName);
                throw new JobExecutionException(string.Format("{0} not specified.", propertyName));
            }
            return value;
        }
    }
}
