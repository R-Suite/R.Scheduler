using System.IO;
using Moq;
using Quartz;
using Quartz.Impl;
using R.Scheduler.PipesAndFilters;
using Xunit;

namespace R.Scheduler.IntegrationTests.PipesAndFilters
{
    public class PafJobRunnerTests
    {
        private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();

        public PafJobRunnerTests()
        {
            // Delete the controle text file if already exists
            File.Delete("FakePafFilter1.txt");
            File.Delete("FakePafFilter2.txt");
            Assert.False(File.Exists("FakePafFilter1.txt"));
            Assert.False(File.Exists("FakePafFilter2.txt"));
        }

        [Fact]
        public void TestExecuteMethodLoadsPluginFromPathAndExecutesIt()
        {
            // Arrange
            var runner = new JobRunner();

            string currentDirectory = Directory.GetCurrentDirectory();
            IJobDetail jobDetail = new JobDetailImpl("jobsettings", typeof(IJob));
            jobDetail.JobDataMap.Add("jobDefinitionPath", Path.Combine(currentDirectory, @"Resourses\Test.Job.config"));
            _mockJobExecutionContext.SetupGet(p => p.JobDetail).Returns(jobDetail);


            // Act
            runner.Execute(_mockJobExecutionContext.Object);

            // Assert
            Assert.True(File.Exists("FakePafFilter1.txt"));
            Assert.True(File.Exists("FakePafFilter2.txt"));
        }
    }
}
