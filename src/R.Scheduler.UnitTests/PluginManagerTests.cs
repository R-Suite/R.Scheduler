using System;
using System.IO;
using Moq;
using Quartz.Job;
using R.Scheduler.AssemblyPlugin;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class PluginManagerTests
    {
        private readonly Mock<ISchedulerCore> _mockSchedulerCore = new Mock<ISchedulerCore>();
        private readonly Mock<ICustomJobStore> _mockPluginStore = new Mock<ICustomJobStore>();

        private class TestCutomJob : ICustomJob
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Params { get; set; }
            public string JobType { get; set; }
        }

        [Fact]
        public void ShouldDeleteJobsAndDeleteCustomJobWhenCalledRemove()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            _mockSchedulerCore.Setup(x => x.RemoveJobGroup(id.ToString()));
            _mockPluginStore.Setup(x => x.GetRegisteredJob(id)).Returns(new TestCutomJob { Id = id });

            IJobTypeManager pluginManager = new PluginManager(_mockPluginStore.Object, _mockSchedulerCore.Object);

            // Act 
            pluginManager.Remove(id);

            // Assert
            _mockPluginStore.Verify(i => i.Remove(id), Times.Once());
            _mockSchedulerCore.Verify(x => x.RemoveJobGroup(id.ToString()));
        }

        [Fact]
        public void ShouldThrowWhenRegisteringCustomJobWithInvalidFilePath()
        {
            // Arrange
            IJobTypeManager pluginManager = new PluginManager(_mockPluginStore.Object, _mockSchedulerCore.Object);

            // Act / Assert 
            Assert.Throws<FileNotFoundException>(() => pluginManager.Register("name1", "invalid\\file\\path"));
        }
    }
}
