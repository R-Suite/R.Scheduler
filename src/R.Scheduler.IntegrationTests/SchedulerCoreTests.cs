using System;
using System.Collections.Generic;
using Moq;
using Quartz;
using Quartz.Job;
using R.Scheduler.Contracts.Model;
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

            const string jobName = "Job1";
            const string jobGroup = "Group1";

            // Act
            var result = _schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);

            // Assert
            Assert.Equal(jobName, persistanceStore.GetJobKey(result).Name);
            Assert.Equal(jobGroup, persistanceStore.GetJobKey(result).Group);
        }

        [Fact]
        public void TestScheduleTriggerWithJobDataMapSavesDataOnTrigger()
        {
            // Arrange
            Scheduler.Shutdown();
            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
            }));
            IPersistanceStore persistanceStore = new InMemoryStore();
            _schedulerCore = new SchedulerCore(Scheduler.Instance(), persistanceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            const string triggerName = "Trigger1";

            _schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);

            var simpleTrigger = new SimpleTrigger
            {
                RepeatCount = 1,
                RepeatInterval = new TimeSpan(0, 1, 0, 0),
                JobName = jobName,
                JobGroup = jobGroup,
                Name = triggerName,
                JobDataMap = new Dictionary<string, object> {{"Key1", "Value1"}}
            };

            // Act
            var result = _schedulerCore.ScheduleTrigger(simpleTrigger);

            // Assert
            Assert.Equal(triggerName, Scheduler.Instance().GetTrigger(new TriggerKey(triggerName)).Key.Name);
            Assert.Equal("Value1", Scheduler.Instance().GetTrigger(new TriggerKey(triggerName)).JobDataMap.GetString("Key1"));
        }

        [Fact]
        public void TestScheduleTriggerWithNoJobDataMapSavesEmptyDataOnTrigger()
        {
            // Arrange
            Scheduler.Shutdown();
            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
            }));
            IPersistanceStore persistanceStore = new InMemoryStore();
            _schedulerCore = new SchedulerCore(Scheduler.Instance(), persistanceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            const string triggerName = "Trigger1";

            _schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty);

            var simpleTrigger = new SimpleTrigger
            {
                RepeatCount = 1,
                RepeatInterval = new TimeSpan(0, 1, 0, 0),
                JobName = jobName,
                JobGroup = jobGroup,
                Name = triggerName
            };

            // Act
            var result = _schedulerCore.ScheduleTrigger(simpleTrigger);

            // Assert
            Assert.Equal(triggerName, Scheduler.Instance().GetTrigger(new TriggerKey(triggerName)).Key.Name);
            Assert.Equal(0, Scheduler.Instance().GetTrigger(new TriggerKey(triggerName)).JobDataMap.Count);
        }
    }
}
