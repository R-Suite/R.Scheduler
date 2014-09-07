using System;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Persistance;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class InMemoryPluginStoreTests
    {
        [Fact]
        public void ShouldGetRegisteredPlugin()
        {
            // Arrange
            var plugin = new Plugin { Name = "TestPlugin", AssemblyPath = "TestsAssemblyPath"};
            IPluginStore pluginStore = new InMemoryPluginStore();
            pluginStore.RegisterPlugin(plugin);

            // Act 
            var result = pluginStore.GetRegisteredPlugin("TestPlugin");

            // Assert
            Assert.Equal("TestsAssemblyPath", result.AssemblyPath);
        }

        [Fact]
        public void ShouldReturnNullWhenGettingUnRegisteredPlugin()
        {
            // Arrange
            IPluginStore pluginStore = new InMemoryPluginStore();

            // Act 
            var result = pluginStore.GetRegisteredPlugin("APlugin");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ShouldUpdatePreviouslyRegisteredPlugin()
        {
            // Arrange
            var plugin = new Plugin { Name = "TestPlugin", AssemblyPath = "TestsAssemblyPath" };
            var plugin2 = new Plugin { Name = "TestPlugin", AssemblyPath = "TestsAssemblyPath2" };
            IPluginStore pluginStore = new InMemoryPluginStore();
            pluginStore.RegisterPlugin(plugin);

            // Act 
            pluginStore.RegisterPlugin(plugin2);
            var result = pluginStore.GetRegisteredPlugin("TestPlugin");

            // Assert
            Assert.Equal("TestsAssemblyPath2", result.AssemblyPath);
        }

        [Fact]
        public void ShouldGetAllRegisteredPlugins()
        {
            // Arrange
            var plugin = new Plugin { Name = "TestPlugin", AssemblyPath = "TestsAssemblyPath" };
            var plugin2 = new Plugin { Name = "TestPlugin2", AssemblyPath = "TestsAssemblyPath2" };
            IPluginStore pluginStore = new InMemoryPluginStore();
            pluginStore.RegisterPlugin(plugin);
            pluginStore.RegisterPlugin(plugin2);

            // Act 
            var result = pluginStore.GetRegisteredPlugins();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
