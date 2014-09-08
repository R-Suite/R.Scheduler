using Moq;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class SchedulerCoreTests
    {
        private readonly Mock<IPluginStore> _mockJobExecutionContext = new Mock<IPluginStore>();

        [Fact]
        public void TestExecutePlugin()
        {
            // Arrange
            _mockJobExecutionContext.Setup(x => x.GetRegisteredPlugin("TestPlugin"))
                .Returns(new Plugin {Name = "Test", AssemblyPath = @"Resourses\R.Scheduler.FakeJobPlugin.dll"});

            ISchedulerCore schedulerCore = new SchedulerCore(_mockJobExecutionContext.Object);

            // act / Assert
            Assert.DoesNotThrow(() => schedulerCore.ExecutePlugin("TestPlugin"));
        }
    }
}
