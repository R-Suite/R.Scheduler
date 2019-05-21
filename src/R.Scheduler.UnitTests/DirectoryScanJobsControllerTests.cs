using System.Collections.Generic;
using Moq;
using R.Scheduler.Contracts.JobTypes.DirectoryScan.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class DirectoryScanJobsControllerTests
    {
        private readonly Mock<ISchedulerCore> _mockSchedulerCore = new Mock<ISchedulerCore>();
        private readonly IContainer _container = new Container();

        public DirectoryScanJobsControllerTests()
        {
            _container.Configure(c => c.For<ISchedulerCore>().Use(_mockSchedulerCore.Object));
        }

        [Fact]
        public void CreateNewJobShouldReturnInvalidResponseWhenCallbackUrlIsInvalid()
        {
            // Arrange
            var mockPermissionsHelper = new Mock<IPermissionsHelper>();
            mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string>{"*"});
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            var controller = new DirectoryScanJobsController(mockPermissionsHelper.Object, mockSchedulerCore.Object);

            var model = new DirectoryScanJob {JobName = "TestJob", CallbackUrl = "notvalidurl"};
             
            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.False(result.Valid);
        }

        [Fact]
        public void CreateNewJobShouldReturnInvalidResponseWhenCallbackUrlIsMissing()
        {
            // Arrange
            var mockPermissionsHelper = new Mock<IPermissionsHelper>();
            mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            var controller = new DirectoryScanJobsController(mockPermissionsHelper.Object, mockSchedulerCore.Object);

            var model = new DirectoryScanJob {JobName = "TestJob"};
            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.False(result.Valid);
        }

        [Fact]
        public void CreateNewJobShouldReturnValidResponseWhenCallbackUrlIsValid()
        {
            // Arrange
            var mockPermissionsHelper = new Mock<IPermissionsHelper>();
            mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            var controller = new DirectoryScanJobsController(mockPermissionsHelper.Object, mockSchedulerCore.Object);

            var model = new DirectoryScanJob {JobName = "TestJob", CallbackUrl = "http://valid.com/test/"};

            // Act 
            var result = controller.Post(model);

            // Assert
            Assert.True(result.Valid);
        }
    }
}
