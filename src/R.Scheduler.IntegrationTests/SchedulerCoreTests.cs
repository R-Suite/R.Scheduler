using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class SchedulerCoreTests
    {
        private ISchedulerCore _schedulerCore;
        private readonly Mock<IScheduler> _mockScheduler = new Mock<IScheduler>();

        [Fact]
        public void TestCreateJobWithInMemoryPersistance()
        {
            // Arrange
            IPersistanceStore persistanceStore = new InMemoryStore();
            _schedulerCore = new SchedulerCore(_mockScheduler.Object, persistanceStore);

            // Act
            var result = _schedulerCore.CreateJob("Job1", "Group1", typeof (NoOpJob), new Dictionary<string, object>(), "test job 1");

            // Assert
            Assert.Equal("Job1", persistanceStore.GetJobKey(result).Name);
            Assert.Equal("Group1", persistanceStore.GetJobKey(result).Group);
        }
    }
}
