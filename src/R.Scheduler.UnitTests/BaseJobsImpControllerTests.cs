using System;
using System.Collections.Generic;
using Moq;
using R.Scheduler.Contracts.JobTypes;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;
using Xunit;

namespace R.Scheduler.UnitTests
{
    /// <summary>
    /// Proxy class for testing abstract BaseJobsImpController class
    /// </summary>
    public class TestController : BaseJobsImpController
    {
        public new QueryResponse CreateJob(BaseJob model, Type jobType, Dictionary<string, object> dataMap)
        {
            return base.CreateJob(model, jobType, dataMap);
        }
    }

    public class BaseJobsImpControllerTests
    {
        private readonly Mock<ISchedulerCore> _mockSchedulerCore = new Mock<ISchedulerCore>();

        public BaseJobsImpControllerTests()
        {
            ObjectFactory.Configure(c => c.For<ISchedulerCore>().Use(_mockSchedulerCore.Object));
        }

        [Fact]
        public void CreateJobShouldReturnErrorQueryResponseWhenExceptionIsThrownInSchedulerCore()
        {
            // Arrange
            var controller = new TestController();
            _mockSchedulerCore.Setup(i => i.CreateJob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), null)).Throws(new Exception());

            // Act 
            var result = controller.CreateJob(null, null, null);

            // Assert
            Assert.False(result.Valid);
            Assert.NotNull(result.Errors);
            Assert.Equal(1, result.Errors.Count);
            Assert.Equal("ErrorCreatingJob", result.Errors[0].Code);
            Assert.Equal("Server", result.Errors[0].Type);
        }
    }
}
