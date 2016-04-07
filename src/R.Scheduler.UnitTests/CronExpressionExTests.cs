using System;
using Moq;
using Quartz;
using Quartz.Impl.Calendar;
using R.Scheduler.Core;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class CronExpressionExTests
    {
        private readonly Mock<IScheduler> _mockScheduler = new Mock<IScheduler>();

        [Fact]
        public void ShouldGetArrayOfFutureFireDateTimesForCronString()
        {
            // Arrange
            var csu = new CronExpressionEx("0 0 12 ? * MON-FRI *", _mockScheduler.Object);
            var dateTimeAfter = new DateTime(2016, 1, 1, 12, 0, 0);

            // Act 
            var result = csu.GetFutureFireDateTimesUtcAfter(dateTimeAfter, 10);

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(new DateTime(2016, 1, 4, 0, 0, 0), result[0].Date);
            Assert.Equal(new DateTime(2016, 1, 15, 0, 0, 0), result[9].Date);
        }


        [Fact]
        public void ShouldGetArrayOfFutureFireDateTimesForCronStringExcludingCalendarDates()
        {
            // Arrange
            const string calName = "TestCal1";
            var cal = new HolidayCalendar();
            cal.AddExcludedDate(new DateTime(2016, 1, 4, 12, 0, 0));

            _mockScheduler.Setup(i => i.GetCalendar(calName)).Returns(cal);

            var csu = new CronExpressionEx("0 0 12 ? * MON-FRI *", _mockScheduler.Object);
            var dateTimeAfter = new DateTime(2016, 1, 1, 12, 0, 0);

            // Act 
            var result = csu.GetFutureFireDateTimesUtcAfter(dateTimeAfter, 10, calName);

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(new DateTime(2016, 1, 5, 0, 0, 0), result[0].Date);
            Assert.Equal(new DateTime(2016, 1, 18, 0, 0, 0), result[9].Date);
        }
    }
}
