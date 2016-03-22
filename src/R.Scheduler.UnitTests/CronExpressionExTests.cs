using System;
using R.Scheduler.Core;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class CronExpressionExTests
    {
        [Fact]
        public void ShouldGetArrayOfFutureFireDateTimesForCronString()
        {
            // Arrange
            var csu = new CronExpressionEx("0 0 12 ? * MON-FRI *");
            var dateTimeAfter = new DateTime(2016, 1, 1, 12, 0, 0);

            // Act 
            var result = csu.GetFutureFireDateTimesUtcAfter(dateTimeAfter, 10);

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(new DateTime(2016, 1, 4, 0, 0, 0), result[0].Date);
            Assert.Equal(new DateTime(2016, 1, 15, 0, 0, 0), result[9].Date);
        }
    }
}
