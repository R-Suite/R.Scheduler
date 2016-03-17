using System.Collections.Generic;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class SchedulerTests
    {
        [Fact]
        public void TestSchedulerInitializesWithDefaultValues()
        {
            // Arrange
            Scheduler.Shutdown();
            
            // Act
            Scheduler.Initialize(_=>_.AutoStart = false);

            // Assert
            Assert.True(Scheduler.Configuration.EnableAuditHistory);
            Assert.True(Scheduler.Configuration.EnableWebApiSelfHost);
            Assert.Equal(PersistanceStoreType.InMemory, Scheduler.Configuration.PersistanceStoreType);
            Assert.Equal("QRTZ_", Scheduler.Configuration.TablePrefix);
            Assert.Equal("RScheduler", Scheduler.Configuration.InstanceName);
            Assert.Equal("instance_one", Scheduler.Configuration.InstanceId);
            Assert.Equal("false", Scheduler.Configuration.UseProperties);
            Assert.Equal("http://localhost:5000/", Scheduler.Configuration.WebApiBaseAddress);
        }


        [Fact]
        public void TestSchedulerInitializesWithCustomValues()
        {
            // Arrange
            Scheduler.Shutdown();

            // Act
            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistanceStoreType = PersistanceStoreType.Postgre;
                config.TablePrefix = "TEST_";
                config.InstanceName = "TestInstance";
                config.InstanceId = "TestInstanceId";
                config.UseProperties = "true";
                config.WebApiBaseAddress = "http://test:123/";
                config.AutoStart = false;
            }));

            // Assert
            Assert.False(Scheduler.Configuration.EnableAuditHistory);
            Assert.False(Scheduler.Configuration.EnableWebApiSelfHost);
            Assert.False(Scheduler.Configuration.AutoStart);
            Assert.Equal(PersistanceStoreType.Postgre, Scheduler.Configuration.PersistanceStoreType);
            Assert.Equal("TEST_", Scheduler.Configuration.TablePrefix);
            Assert.Equal("TestInstance", Scheduler.Configuration.InstanceName);
            Assert.Equal("TestInstanceId", Scheduler.Configuration.InstanceId);
            Assert.Equal("true", Scheduler.Configuration.UseProperties);
            Assert.Equal("http://test:123/", Scheduler.Configuration.WebApiBaseAddress);
        }

        [Fact]
        public void TestSchedulerInitializesWithCustomListeners()
        {
            // Arrange
            Scheduler.Shutdown();

            // Act
            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.CustomTriggerListenerAssemblyNames = new List<string> { "R.Scheduler.TestListenersImp.dll" };
                config.CustomJobListenerAssemblyNames = new List<string> { "R.Scheduler.TestListenersImp.dll" };
                config.CustomSchedulerListenerAssemblyNames = new List<string> { "R.Scheduler.TestListenersImp.dll" };
            }));

            // Assert
            Assert.NotNull(Scheduler.Instance().ListenerManager.GetTriggerListener("MyTestTriggerListener"));
            Assert.NotNull(Scheduler.Instance().ListenerManager.GetJobListener("MyTestJobListener"));
            Assert.Equal(1, Scheduler.Instance().ListenerManager.GetSchedulerListeners().Count);
        }
    }
}
