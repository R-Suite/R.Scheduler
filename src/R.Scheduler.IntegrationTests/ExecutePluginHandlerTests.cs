using System;
using Moq;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Contracts.Messages;
using R.Scheduler.Handlers;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class ExecutePluginHandlerTests
    {
        private readonly Mock<IPluginStore> _mockJobExecutionContext = new Mock<IPluginStore>();

        [Fact]
        public void TestExecute()
        {
            // Arrange
            _mockJobExecutionContext.Setup(x => x.GetRegisteredPlugin("TestPlugin"))
                .Returns(new Plugin {Name = "Test", AssemblyPath = @"Resourses\R.Scheduler.FakeJobPlugin.dll"});
            var handler = new ExecutePluginHandler(_mockJobExecutionContext.Object);

            // act / Assert
            Assert.DoesNotThrow(() => handler.Execute(new ExecutePlugin(Guid.NewGuid()) { PluginName = "TestPlugin" }));
        }
    }
}
