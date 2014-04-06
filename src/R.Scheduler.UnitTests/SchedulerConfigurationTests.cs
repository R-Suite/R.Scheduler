using System;
using R.Scheduler.Contracts.Interfaces;
using StructureMap;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class FakePluginStore : IPluginStore
    {
        public Plugin GetRegisteredPlugin(string pluginName)
        {
            throw new NotImplementedException();
        }

        public void RegisterPlugin(Plugin plugin)
        {
            throw new NotImplementedException();
        }
    }

    public class SchedulerConfigurationTests
    {
        [Fact]
        public void SetPluginStoreShouldOverrideDefaultIPluginStoreInstance()
        {
            // Arrange
            Scheduler.Instance();

            // Act
            Scheduler.SetPluginStore<FakePluginStore>();

            // Assert
            Assert.Equal(typeof(FakePluginStore),  ObjectFactory.GetInstance<IPluginStore>().GetType());
        }
    }
}
