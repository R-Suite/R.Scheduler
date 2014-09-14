using System;
using System.Collections.Generic;
using Moq;
using Quartz;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class SchedulerCoreTests
    {
        private readonly Mock<IScheduler> _mockScheduler = new Mock<IScheduler>();
        private readonly Mock<IPluginStore> _mockPluginStore = new Mock<IPluginStore>();

        [Fact]
        public void ShouldScheduleJobWithSimpleTriggerWhenCalledExecutePlugin()
        {
            // Arrange
            _mockPluginStore.Setup(x => x.GetRegisteredPlugin("TestPlugin")).Returns(new Plugin { Name = "Test", AssemblyPath = @"TestPath.dll" });

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.ExecuteJob(typeof(IJob), new Dictionary<string, object> { { "pluginPath", "TestPath.dll" } });

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<IJobDetail>(j => j.JobDataMap.ContainsKey("pluginPath") && j.JobDataMap.ContainsValue("TestPath.dll")),
                It.Is<ISimpleTrigger>(t => t.RepeatCount == 0)));
        }

        [Fact]
        public void ShouldDeleteJobInAllJobGroupsWhenJobGroupIsNotSpecifiedInRemoveJob()
        {
            // Arrange
            _mockScheduler.Setup(x => x.GetJobGroupNames()).Returns(new List<string> { "Group1", "Group2" });
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);


            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.RemoveJob("TestJob");

            // Assert
            _mockScheduler.Verify(x => x.DeleteJob(It.Is<JobKey>(i => i.Name == "TestJob")),Times.Exactly(2));
        }

        [Fact]
        public void ShouldDeleteJobInJobGroupWhenJobGroupIsSpecifiedInRemoveJob()
        {
            // Arrange
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(true);


            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.RemoveJob("TestJob", "Group1");

            // Assert
            _mockScheduler.Verify(x => x.DeleteJob(It.Is<JobKey>(i => i.Name == "TestJob")), Times.Once);
            _mockScheduler.Verify(x => x.GetJobGroupNames(), Times.Never);
        }

        [Fact]
        public void ShouldDeleteTriggerInAllTriggerGroupsWhenTriggerGroupIsNotSpecifiedInRemoveTrigger()
        {
            // Arrange
            _mockScheduler.Setup(x => x.GetTriggerGroupNames()).Returns(new List<string> { "Group1", "Group2" });
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<TriggerKey>())).Returns(true);


            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.RemoveTrigger("TestTrigger");

            // Assert
            _mockScheduler.Verify(x => x.UnscheduleJob(It.Is<TriggerKey>(i => i.Name == "TestTrigger")), Times.Exactly(2));
        }

        [Fact]
        public void ShouldDeleteTriggerInTriggerGroupWhenTriggerGroupIsSpecifiedInRemoveTrigger()
        {
            // Arrange
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<TriggerKey>())).Returns(true);


            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.RemoveTrigger("TestTrigger", "Group1");

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
                RepeatInterval = new TimeSpan(0,0,0,1),
                DataMap = new Dictionary<string, object> { { "pluginPath", "TestPath.dll" } }
            };
            IJobDetail nullJd = null;
            _mockScheduler.Setup(x => x.GetJobDetail(It.IsAny<JobKey>())).Returns(nullJd);
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(false);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);
            
            // Act 
            schedulerCore.ScheduleTrigger(myTrigger, typeof(IJob));

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<IJobDetail>(j => j.JobDataMap.ContainsKey("pluginPath") && j.JobDataMap.ContainsValue("TestPath.dll")),
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
                CronExpression = "0/30 * * * * ?",
                DataMap = new Dictionary<string, object> { { "pluginPath", "TestPath.dll" } }
            };
            IJobDetail nullJd = null;
            _mockScheduler.Setup(x => x.GetJobDetail(It.IsAny<JobKey>())).Returns(nullJd);
            _mockScheduler.Setup(x => x.CheckExists(It.IsAny<JobKey>())).Returns(false);

            ISchedulerCore schedulerCore = new SchedulerCore(_mockScheduler.Object);

            // Act 
            schedulerCore.ScheduleTrigger(myTrigger, typeof(IJob));

            // Assert
            _mockScheduler.Verify(x => x.ScheduleJob(
                It.Is<IJobDetail>(j => j.JobDataMap.ContainsKey("pluginPath") && j.JobDataMap.ContainsValue("TestPath.dll")),
                It.Is<ICronTrigger>(t => t.CronExpressionString == "0/30 * * * * ?")), Times.Once);
        }
    }
}
