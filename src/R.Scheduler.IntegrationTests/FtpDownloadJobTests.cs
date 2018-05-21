using System;
using System.Configuration;
using Moq;
using Quartz;
using Quartz.Impl;
using R.Scheduler.Core;
using R.Scheduler.Ftp;
using StructureMap;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class FtpDownloadJobTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();
        private readonly Mock<IFtpLibrary> _mockFtpLibrary = new Mock<IFtpLibrary>();

        [Fact]
        public void TestExecuteMethodInvokesConnectAndGetFilesMethods()
        {
            // Arrange
            ObjectFactory.Configure(config =>
            {
                config.For<IFtpLibrary>().Use(_mockFtpLibrary.Object);
            });

            var ftpJob = new FtpDownloadJob();

            var plainTextUserName = "myUserName";
            var plainTextPassword = "myPassword";
            var host = "ftp://testhost.com";
            var localDirectoryPath = "C:/";
            var fileExtensions = ".txt";
            var userName = AESGCM.SimpleEncrypt(plainTextUserName, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));
            var password = AESGCM.SimpleEncrypt(plainTextPassword, Convert.FromBase64String(ConfigurationManager.AppSettings["SchedulerEncryptionKey"]));

            //string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("TestFtpDownloadJob1", typeof(IJob));
            jobDetail.JobDataMap.Add("ftpHost", host);
            jobDetail.JobDataMap.Add("localDirectoryPath", localDirectoryPath);
            jobDetail.JobDataMap.Add("fileExtensions", fileExtensions);
            jobDetail.JobDataMap.Add("userName", userName);
            jobDetail.JobDataMap.Add("password", password);
            _mockJobExecutionContext.SetupGet(p => p.MergedJobDataMap).Returns(jobDetail.JobDataMap);
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            ftpJob.Execute(_mockJobExecutionContext.Object);

            // Assert
            _mockFtpLibrary.Verify(i=>i.Connect(host, 21, plainTextUserName, plainTextPassword, null, null), Times.Once);
            _mockFtpLibrary.Verify(i => i.GetFiles(It.IsAny<string>(), localDirectoryPath, fileExtensions, It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
