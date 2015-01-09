using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Common.Logging;
using Quartz;

namespace R.Scheduler.WebRequest
{
    public class WebRequestJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The request action type. REQUIRED.</summary>
        public const string ActionType = "actionType";

        /// <summary> The request method. REQUIRED.</summary>
        public const string Method = "method";

        /// <summary> The request URI. REQUIRED.</summary>
        public const string Uri = "uri";

        /// <summary> The (POST/PUT) request body. Optional.</summary>
        public const string Body = "body";

        /// <summary>
        /// Ctor used by Scheduler engine
        /// </summary>
        public WebRequestJob()
        {
            Logger.Info("Entering WebRequestJob.ctor().");
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;

            string actionType = GetRequiredParameter(data, ActionType);
            string method = GetRequiredParameter(data, Method);
            string uri = GetRequiredParameter(data, Uri);
            string body = GetOptionalParameter(data, Body);

            if (uri.ToLower().StartsWith("http"))
            {
                uri = uri.Replace("http://", "");
                uri = uri.Replace("https://", "");
                uri = uri.Replace("HTTP://", "");
                uri = uri.Replace("HTTPS://", "");
            }

            // Execute WebRequest
            System.Net.WebRequest request = System.Net.WebRequest.Create(actionType + uri);
            request.Method = method;

            Stream dataStream;

            // Create POST/PUT data and convert it to a byte array.
            if ((method.ToUpper() == "PUT" || method.ToUpper() == "POST") && !string.IsNullOrEmpty(body))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            // Get the response.
            WebResponse response = request.GetResponse();
            Logger.Info(((HttpWebResponse)response).StatusDescription);
            Logger.InfoFormat("WebRequestJob server response status: {0}", ((HttpWebResponse)response).StatusDescription);
            
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Logger.InfoFormat("WebRequestJob server response content: {0}", responseFromServer);

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
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
