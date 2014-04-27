using System;
using R.Scheduler.Contracts.Interfaces;
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
            Scheduler.Instance();

            // Act / Assert
            Assert.Throws<Exception>(() => Scheduler.Initialize(c=>c.ConnectionString = "test connection string"));
        }
    }
}
