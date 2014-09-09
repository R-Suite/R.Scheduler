using System;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class SchedulerConfigurationTests
    {
        [Fact]
        public void ShouldThrowWhenSchedulerIsInitializedAfterSchedulerIsStarted()
        {
            // Arrange
            Scheduler.Initialize(c =>
            {
                c.EnableMessageBusSelfHost = false;
                c.EnableWebApiSelfHost = false;
            });

            Scheduler.Instance();

            // Act / Assert
            Assert.Throws<Exception>(() => Scheduler.Initialize(c=>c.ConnectionString = "test connection string"));
        }

        [Fact]
        public void ShouldEnableWebApiSelfHostWhenSchedulerIsStarted()
        {
            // Act
            Scheduler.Initialize(c => { });

            // Assert
            Assert.True(Scheduler.Configuration.EnableWebApiSelfHost);
        }

        [Fact]
        public void ShouldDisableMessageBusSelfHostWhenSchedulerIsStarted()
        {
            // Act
            Scheduler.Initialize(c => { });

            // Assert
            Assert.False(Scheduler.Configuration.EnableMessageBusSelfHost);
        }
    }
}
