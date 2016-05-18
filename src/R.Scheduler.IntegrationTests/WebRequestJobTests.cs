using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Moq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    namespace ClassLibrary.IntegrationTests
    {
        public class TestController : ApiController
        {
            [AcceptVerbs("GET")]
            [Route("testapi/test")]
            public string Get(string jobName, string fireInstanceId)
            {
                return fireInstanceId;
            }
        }
    }

    public class WebRequestJobTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();
        private const string BaseHostingAddress = @"http://localhost:8084";

        [Fact]
        public void TestWebRequestJobPassesFireInstanceIdInTheQueryString()
        {
            const string testFireInstanceId = "123";

            using (WebApp.Start<Startup>(BaseHostingAddress)) // base hosting address
            {
                // Arrange
                var pluginRunner = new WebRequest.WebRequestJob();
                _mockJobExecutionContext.SetupGet(p => p.FireInstanceId).Returns(testFireInstanceId);

                IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof (IJob));
                jobDetail.JobDataMap.Add("actionType", "http");
                jobDetail.JobDataMap.Add("method", "GET");
                jobDetail.JobDataMap.Add("contentType", "text/plain");
                jobDetail.JobDataMap.Add("uri", BaseHostingAddress + "/testapi/test?JobName=TestJob&FireInstanceId={$FireInstanceId}");
                _mockJobExecutionContext.SetupGet(p => p.MergedJobDataMap).Returns(jobDetail.JobDataMap);
                _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

                // Act
                pluginRunner.Execute(_mockJobExecutionContext.Object);

                // Assert
                _mockJobExecutionContext.VerifySet(p => p.Result = "\"" + testFireInstanceId + "\"", Times.Once);
            }
        }
    }
}
