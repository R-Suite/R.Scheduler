using System;
using System.Collections.Generic;
using System.Linq;
using Quartz.Job;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class AnalyticsTests
    {
        private readonly IAnalytics _analytics;
        private readonly ISchedulerCore _schedulerCore;

        public AnalyticsTests()
        {
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistanceStoreType = PersistanceStoreType.InMemory;
                config.AutoStart = false;
            }));

            IPersistanceStore persistanceStore = new InMemoryStore();
            _analytics = new Analytics(Scheduler.Instance(), persistanceStore);
            _schedulerCore = new SchedulerCore(Scheduler.Instance(), persistanceStore);
        }

        [Fact]
        public void TestGetUpcomingJobsReturnsResultsInCorrectOrder()
        {
            // Arrange
            _schedulerCore.CreateJob("TestJob1", string.Empty, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);
            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                JobName = "TestJob1",
                Name = "TestTrigger1",
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0, 1, 0),
                StartDateTime = DateTime.Now.AddMinutes(2)
            });
            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                JobName = "TestJob1",
                Name = "TestTrigger2",
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0, 2, 0),
                StartDateTime = DateTime.Now.AddMinutes(1)
            });

            // Act
            var result = _analytics.GetUpcomingJobs(10).ToList();

            // Assert
            Assert.Equal(8, result.Count);
            Assert.Equal("TestTrigger2", result[0].TriggerName);
            Assert.Equal("TestTrigger1", result[1].TriggerName);
            Assert.Equal("TestTrigger2", result[7].TriggerName);
        }

        [Fact]
        public void TestGetJobCountReturnCorrectResult()
        {
            // Arrange
            _schedulerCore.CreateJob("TestJob1", string.Empty, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);
            _schedulerCore.CreateJob("TestJob2", string.Empty, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);

            // Act
            var result = _analytics.GetJobCount();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void TestGetTriggerCountReturnCorrectResult()
        {
            // Arrange
            _schedulerCore.CreateJob("TestJob1", string.Empty, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);
            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                JobName = "TestJob1",
                Name = "TestTrigger1",
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0, 1, 0)
            });
            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                JobName = "TestJob1",
                Name = "TestTrigger2",
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0, 2, 0)
            });
            _schedulerCore.CreateJob("TestJob2", string.Empty, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);
            _schedulerCore.ScheduleTrigger(new SimpleTrigger
            {
                JobName = "TestJob2",
                Name = "TestTrigger3",
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0, 2, 0)
            });

            // Act
            var result = _analytics.GetTriggerCount();

            // Assert
            Assert.Equal(3, result);
        }
    }
}
