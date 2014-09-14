using System.IO;
using Moq;
using Quartz;
using Quartz.Impl;
using R.Scheduler.AssemblyPlugin;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class PluginRunnerTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();

        [Fact(Skip = "Job runner does not reference bus anymore.")]
        public void TestExecuteMethodLoadsPluginFromPathAndExecutesIt()
        {
            // Arrange
            var pluginRunner = new PluginRunner();

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("pluginPath", Path.Combine(currentDirectory, @"Resourses\R.Scheduler.FakeJobPlugin.dll"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            pluginRunner.Execute(_mockJobExecutionContext.Object);

            // Assert
        }

        [Fact(Skip = "Job runner does not reference bus anymore.")]
        public void TestExecuteMethodReturnsWhenPluginPathIsMissingInJobDataMap()
        {
            // Arrange
            var pluginRunner = new PluginRunner();

            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            pluginRunner.Execute(_mockJobExecutionContext.Object);

            // Assert
        }
    }
}
