using Moq;
using R.Scheduler.AssemblyPlugin;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class PluginManagerTests
    {
        private readonly Mock<ISchedulerCore> _mockSchedulerCore = new Mock<ISchedulerCore>();
        private readonly Mock<IPluginStore> _mockPluginStore = new Mock<IPluginStore>();

        [Fact]
        public void ShouldDeleteJobsAndDeletePluginWhenCalledRemovePlugin()
        {
            // Arrange
            _mockSchedulerCore.Setup(x => x.RemoveJobGroup("TestPlugin"));

            IJobTypeManager pluginManager = new PluginManager(_mockPluginStore.Object, _mockSchedulerCore.Object);

            // Act 
            pluginManager.Remove("TestPlugin");

            // Assert
            _mockPluginStore.Verify(i => i.RemovePlugin("TestPlugin"), Times.Once());
            _mockSchedulerCore.Verify(x => x.RemoveJobGroup("TestPlugin"));
        }
    }
}
