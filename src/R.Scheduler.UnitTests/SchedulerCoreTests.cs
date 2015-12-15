using System;
using System.Collections.Generic;
using Moq;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class SchedulerCoreTests
    {
        private readonly Mock<IScheduler> _mockScheduler = new Mock<IScheduler>();
        private readonly Mock<IPersistanceStore> _mockPersistanceStore = new Mock<IPersistanceStore>();

        [Fact]
        public void ShouldDeleteJobInDefaultJobGroupsWhenJobGroupIsNotSpecifiedInRemoveJob()
        {
            // Arrange
            Guid jobId = Guid.NewGuid();
            _mockScheduler.Setup(x => x.GetJobGroupNames()).Returns(new List<string> { "Group1", "DEFAULT" });
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);
            _mockPersistanceStore.Setup(x => x.GetJobKey(jobId)).Returns(new JobKey("TestJob", "DEFAULT"));

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.RemoveJob(jobId);

            // Assert
            _mockScheduler.Verify(x => x.DeleteJob(It.Is<JobKey>(i => i.Name == "TestJob" && i.Group == "DEFAULT")),Times.Exactly(1));
        }

        [Fact]
        public void ShouldDeleteJobInJobGroupWhenJobGroupIsSpecifiedInRemoveJob()
        {
            // Arrange
            Guid jobId = Guid.NewGuid();
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);
            _mockPersistanceStore.Setup(x => x.GetJobKey(jobId)).Returns(new JobKey("TestJob", "Group1"));


            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.RemoveJob(jobId);

            // Assert
            _mockScheduler.Verify(x => x.DeleteJob(It.Is<JobKey>(i => i.Name == "TestJob")), Times.Once);
            _mockScheduler.Verify(x => x.GetJobGroupNames(), Times.Never);
        }

        [Fact]
        public void ShouldDeleteTriggerInDefaultTriggerGroupsWhenTriggerGroupIsNotSpecifiedInRemoveTrigger()
        {
            // Arrange
            Guid triggerId = Guid.NewGuid();
            _mockPersistanceStore.Setup(x => x.GetTriggerKey(triggerId)).Returns(new TriggerKey("TestTrigger", "DEFAULT"));
            _mockScheduler.Setup(x => x.GetTriggerGroupNames()).Returns(new List<string> { "DEFAULT", "Group2" });
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<TriggerKey>())).Returns(true);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.RemoveTrigger(triggerId);

            // Assert
            _mockScheduler.Verify(x => x.UnscheduleJob(It.Is<TriggerKey>(i => i.Name == "TestTrigger" && i.Group == "DEFAULT")), Times.Exactly(1));
        }

        [Fact]
        public void ShouldDeleteTriggerInTriggerGroupWhenTriggerGroupIsSpecifiedInRemoveTrigger()
        {
            // Arrange
            Guid triggerId = Guid.NewGuid();
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<TriggerKey>())).Returns(true);
            _mockPersistanceStore.Setup(x => x.GetTriggerKey(triggerId)).Returns(new TriggerKey("TestTrigger", "Group1"));
            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.RemoveTrigger(triggerId);

            // Assert
            _mockScheduler.Verify(x => x.UnscheduleJob(It.Is<TriggerKey>(i => i.Name == "TestTrigger")), Times.Once);
            _mockScheduler.Verify(x => x.GetTriggerGroupNames(), Times.Never);
        }

        [Fact]
        public void ShouldScheduleJobWithSimpleTriggerWhenCalledScheduleTrigger()
        {
            // Arrange
            var myTrigger = new SimpleTrigger
            {
                Name = "TestTrigger",
                Group = "TestGroup",
                JobName = "TestJobName",
                JobGroup = "TestJobGroup",
                RepeatCount = 2,
                RepeatInterval = new TimeSpan(0,0,0,1)
            };
            IJobDetail noOpJob = new JobDetailImpl("TestJobName", "TestJobGroup", typeof(NoOpJob));
            _mockScheduler.Setup(x => x.GetJobDetail(It.IsAny<JobKey>())).Returns(noOpJob);
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);
            
            // Act 
            schedulerCore.ScheduleTrigger(myTrigger);

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<ISimpleTrigger>(t => t.RepeatCount == 2)), Times.Once);
        }

        [Fact]
        public void ShouldScheduleJobWithCronTriggerWhenCalledScheduleTrigger()
        {
            // Arrange
            var myTrigger = new CronTrigger
            {
                Name = "TestTrigger",
                Group = "TestGroup",
                JobName = "TestJobName",
                JobGroup = "TestJobGroup",
                CronExpression = "0/30 * * * * ?"
            };
            IJobDetail noOpJob = new JobDetailImpl("TestJobName", "TestJobGroup", typeof(NoOpJob));
            _mockScheduler.Setup(x => x.GetJobDetail(It.IsAny<JobKey>())).Returns(noOpJob);
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.ScheduleTrigger(myTrigger);

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<ICronTrigger>(t => t.CronExpressionString == "0/30 * * * * ?")), Times.Once);
        }

        [Fact]
        public void ShouldSetLocalTimezoneOnCronTriggerWhenCalledScheduleTrigger()
        {
            // Arrange
            var myTrigger = new CronTrigger
            {
                Name = "TestTrigger",
                Group = "TestGroup",
                JobName = "TestJobName",
                JobGroup = "TestJobGroup",
                CronExpression = "0/30 * * * * ?"
            };
            IJobDetail noOpJob = new JobDetailImpl("TestJobName", "TestJobGroup", typeof(NoOpJob));
            _mockScheduler.Setup(x => x.GetJobDetail(It.IsAny<JobKey>())).Returns(noOpJob);
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object, _mockPersistanceStore.Object);

            // Act 
            schedulerCore.ScheduleTrigger(myTrigger);

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<ICronTrigger>(t => (Equals(t.TimeZone, TimeZoneInfo.Local)))), Times.Once);
        }
    }
}
