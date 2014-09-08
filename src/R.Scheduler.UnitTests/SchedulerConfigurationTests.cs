using System;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class SchedulerConfigurationTests
    {
        //note: methods are static, the Scheduler class is not really testable

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
    }
}
