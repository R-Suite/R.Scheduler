using Moq;
using R.Scheduler.Contracts.JobTypes.DirectoryScan.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using StructureMap;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class DirectoryScanJobsControllerTests
    {
        private readonly Mock<ISchedulerCore> _mockSchedulerCore = new Mock<ISchedulerCore>();

        public DirectoryScanJobsControllerTests()
        {
            ObjectFactory.Configure(c => c.For<ISchedulerCore>().Use(_mockSchedulerCore.Object));
        }

        [Fact]
        public void CreateNewJobShouldReturnInvalidResponseWhenCallbackUrlIsInvalid()
        {
            // Arrange
            var controller = new DirectoryScanJobsController();
            var model = new DirectoryScanJob();
            model.JobName = "TestJob";
            model.CallbackUrl = "notvalidurl";

            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.False(result.Valid);
        }

        [Fact]
        public void CreateNewJobShouldReturnInvalidResponseWhenCallbackUrlIsMissing()
        {
            // Arrange
            var controller = new DirectoryScanJobsController();
            var model = new DirectoryScanJob();
            model.JobName = "TestJob";

            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.False(result.Valid);
        }

        [Fact]
        public void CreateNewJobShouldReturnValidResponseWhenCallbackUrlIsValid()
        {
            // Arrange
            var controller = new DirectoryScanJobsController();
            var model = new DirectoryScanJob();
            model.JobName = "TestJob";
            model.CallbackUrl = "http://valid.com/test/";

            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.True(result.Valid);
        }
    }
}
