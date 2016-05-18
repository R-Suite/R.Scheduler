using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary> The content type. Optional.</summary>
        public const string ContentType = "contentType";

        /// <summary>
        /// Ctor used by Scheduler engine
        /// </summary>
        public WebRequestJob()
        {
            Logger.Debug("Entering WebRequestJob.ctor().");
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;
            var jobName = context.JobDetail.Key.Name;

            string actionType = GetRequiredParameter(data, ActionType, jobName);
            string method = GetRequiredParameter(data, Method, jobName);
            string uri = GetRequiredParameter(data, Uri, jobName);
            string body = GetOptionalParameter(data, Body) ?? string.Empty;
            string contentType = GetOptionalParameter(data, ContentType) ?? "text/plain";

            // Parse tokens in uri. This allows passing context property values, such as FireInstanceId, in the query string
            var r = new Regex(Regex.Escape("{$") + "(.*?)" + Regex.Escape("}"));
            MatchCollection matches = r.Matches(uri);
            foreach (object match in matches)
            {
                var token = match.ToString();
                var tokenValue = token.Replace("{$", "").Replace("}", "");

                PropertyInfo prop = context.GetType().GetProperty(tokenValue, BindingFlags.Public | BindingFlags.Instance);
                if (null != prop && prop.CanRead)
                {
                    var val = prop.GetValue(context);

                    if (null != val)
                    {
                        uri = uri.Replace(token, val.ToString());
                    }
                }
            }

            if (uri.ToLower().StartsWith("http"))
            {
                uri = uri.Replace("http://", "");
                uri = uri.Replace("https://", "");
                uri = uri.Replace("HTTP://", "");
                uri = uri.Replace("HTTPS://", "");
            }

            if (!actionType.Contains("://"))
            {
                actionType += "://";
            }

            // Execute WebRequest
            System.Net.WebRequest request = System.Net.WebRequest.Create(actionType + uri);
            request.Method = method;

            Stream dataStream;

            // Create POST/PUT data and convert it to a byte array.
            if (method.ToUpper() == "PUT" || method.ToUpper() == "POST")
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                request.ContentType = contentType;
                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            // Get the response.
            try
            {
                WebResponse response = request.GetResponse();
                Logger.InfoFormat("WebRequestJob server response status: {0}", ((HttpWebResponse) response).StatusDescription);

                // Get the stream containing content returned by the server.
                using (dataStream = response.GetResponseStream())
                {
                    if (dataStream != null)
                    {
                        var reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        context.Result = responseFromServer;
                        Logger.InfoFormat("WebRequestJob server response content: {0}", responseFromServer);

                        reader.Close();
                    }
                }
                response.Close();
            }
            catch (WebException ex)
            {
                Logger.Error(string.Format("Error in WebRequestJob ({0}):", jobName), ex);
                context.Result = ex.Message;

                if (ex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse) ex.Response)
                    {
                        var stream = errorResponse.GetResponseStream();
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string error = reader.ReadToEnd();
                                Logger.ErrorFormat("Error in WebRequestJob ({0}): {1}", jobName, error);
                            }
                        }
                    }
                }
                throw new JobExecutionException(ex.Message, ex, false);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error in WebRequestJob ({0}):", jobName), ex);
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
                Logger.ErrorFormat("Error in WebRequestJob ({0}): {1} not specified.", jobName, propertyName);
                throw new JobExecutionException(string.Format("{0} not specified.", propertyName));
            }
            return value;
        }
    }
}
