using System;
using System.IO;
using Moq;
using Quartz;
using Quartz.Impl;
using R.Scheduler.AssemblyPlugin;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    /// <summary>
    /// Execution of the FakeJobPlugin generates an empty control text file.
    /// Presense of the file after each test indicates a successful execution.
    /// 
    /// </summary>
    public class AssemblyPluginJobTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();

        public AssemblyPluginJobTests()
        {
            // Delete the controle text file if already exists
            File.Delete("FakeJobPlugin.txt");
            Assert.False(File.Exists("FakeJobPlugin.txt"));
        }

        [Fact]
        public void TestExecuteMethodLoadsPluginFromPathAndExecutesIt()
        {
            // Arrange
            var pluginRunner = new AssemblyPluginJob();

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("pluginPath", Path.Combine(currentDirectory, @"Resourses\R.Scheduler.FakeJobPlugin.dll"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            pluginRunner.Execute(_mockJobExecutionContext.Object);

            // Assert
            Assert.True(File.Exists("FakeJobPlugin.txt"));
        }

        [Fact]
        public void TestExecuteMethodReturnsWhenPluginPathIsMissingInJobDataMap()
        {
            // Arrange
            var pluginRunner = new AssemblyPluginJob();

            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act / Assert
            Assert.Throws<JobExecutionException>(() => pluginRunner.Execute(_mockJobExecutionContext.Object));
        }

        [Fact]
        public void TestJobFailsWhenPluginPathIsInvalid()
        {
            // Arrange
            var pluginRunner = new AssemblyPluginJob();

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("pluginPath", Path.Combine(currentDirectory, @"Resourses\R.Scheduler.InvalidJobPlugin.dll"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act / Assert
            Assert.Throws<JobExecutionException>(() => pluginRunner.Execute(_mockJobExecutionContext.Object));
        }

        [Fact]
        public void TestJobFailsWhenPluginThrowsErrorDuringExecution()
        {
            // Arrange
            var pluginRunner = new AssemblyPluginJob();

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("pluginPath", Path.Combine(currentDirectory, @"Resourses\R.Scheduler.FakeJobPluginWithError.dll"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act / Assert
            Assert.Throws<JobExecutionException>(() => pluginRunner.Execute(_mockJobExecutionContext.Object));
        }
    }
}
