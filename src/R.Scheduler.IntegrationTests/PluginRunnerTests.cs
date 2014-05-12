using System.IO;
using Moq;
using Quartz;
using Quartz.Impl;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.JobRunners;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class PluginRunnerTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();
        private readonly Mock<IBus> _mockBus= new Mock<IBus>();

        [Fact]
        public void TestExecuteMethodLoadsPluginFromPathAndExecutesIt()
        {
            // Arrange
            _mockBus.Setup(p => p.Publish(It.IsAny<JobExecutedMessage>()));

            var pluginRunner = new PluginRunner(_mockBus.Object);

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("pluginPath", Path.Combine(currentDirectory, @"Resourses\R.Scheduler.FakeJobPlugin.dll"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            pluginRunner.Execute(_mockJobExecutionContext.Object);

            // Assert
            _mockBus.Verify(p => p.Publish(It.Is<JobExecutedMessage>(i => i.Success && i.Type == "R.Scheduler.FakeJobPlugin.Plugin")), Times.Once);
        }

        [Fact]
        public void TestExecuteMethodReturnsWhenPluginPathIsMissingInJobDataMap()
        {
            // Arrange
            var pluginRunner = new PluginRunner(_mockBus.Object);

            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);

            // Act
            pluginRunner.Execute(_mockJobExecutionContext.Object);

            // Assert
            _mockBus.Verify(p => p.Publish(It.IsAny<JobExecutedMessage>()), Times.Never());
        }
    }
}
