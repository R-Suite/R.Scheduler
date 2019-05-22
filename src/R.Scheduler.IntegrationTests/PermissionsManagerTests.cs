using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Moq;
using Quartz.Impl;
using Quartz.Job;
using R.Scheduler.Controllers;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistence;
using Xunit;

namespace R.Scheduler.IntegrationTests
{
    public class PermissionsManagerTests
    {
        private readonly Mock<IPermissionsHelper> _mockPermissionsHelper = new Mock<IPermissionsHelper>();

        #region Jobs Controller Tests

        [Fact]
        public void TestJobsControllerGetByIdThrowsUnauthorized()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act amd Assert
            try
            {
                controller.Get(jobId);
                
            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }
        }

        [Fact]
        public void TestJobsControllerGetByIdWithWildcard()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            Guid jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);

            
            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            JobsController controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = controller.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }

        [Fact]
        public void TestJobsControllerGetByIdWithJobGroupName()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = controller.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }

        [Fact]
        public void TestJobsControllerDeleteByIdWithJobGroupName()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var jobBeforeDelete = controller.Get(jobId);
            controller.Delete(jobId);
            try
            {
                controller.Get(jobId);
            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
            }

            // Assert
            Assert.Equal(jobGroup, jobBeforeDelete.JobGroup);
            Assert.Equal(jobName, jobBeforeDelete.JobName);
        }

        [Fact]
        public void TestJobsControllerDeleteByIdWithWildcard()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            var controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var jobBeforeDelete = controller.Get(jobId);
            controller.Delete(jobId);
            try
            {
                controller.Get(jobId);
            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
            }

            // Assert
            Assert.Equal(jobGroup, jobBeforeDelete.JobGroup);
            Assert.Equal(jobName, jobBeforeDelete.JobName);
        }

        [Fact]
        public void TestJobsControllerDeleteUnauthorized()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var controller = new JobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            
            try
            {
                controller.Delete(jobId);
            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }

        }

        [Fact]
        public void TestJobsControllerExecuteUnauthorized()
        {
            // Arrange
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            mockSchedulerCore.Setup(i => i.GetJobDetail(It.IsAny<Guid>()))
                .Returns(new JobDetailImpl("Job1", "Group1", typeof(NoOpJob)));
            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var controller = new JobsController(_mockPermissionsHelper.Object, mockSchedulerCore.Object);

            // Assert
            try
            {
                // Act
                controller.Execute(Guid.NewGuid());
            }
            catch (HttpResponseException ex)
            {
                // Assert
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }

        }

        [Fact]
        public void TestJobsControllerExecuteWithGroupName()
        {
            // Arrange
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            mockSchedulerCore.Setup(i => i.GetJobDetail(It.IsAny<Guid>()))
                .Returns(new JobDetailImpl("Job1", "Group1", typeof(NoOpJob)));
            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var controller = new JobsController(_mockPermissionsHelper.Object, mockSchedulerCore.Object);

            // Act
            controller.Execute(Guid.NewGuid());

            // Assert
            mockSchedulerCore.Verify(i => i.ExecuteJob(It.IsAny<Guid>()), Times.Once);

        }

        [Fact]
        public void TestJobsControllerExecuteWithWildcard()
        {
            // Arrange
            Mock<ISchedulerCore> mockSchedulerCore = new Mock<ISchedulerCore>();

            mockSchedulerCore.Setup(i => i.GetJobDetail(It.IsAny<Guid>()))
                .Returns(new JobDetailImpl("Job1", "Group1", typeof(NoOpJob)));
            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            var controller = new JobsController(_mockPermissionsHelper.Object, mockSchedulerCore.Object);

            // Act
            controller.Execute(Guid.NewGuid());

            // Assert
            mockSchedulerCore.Verify(i => i.ExecuteJob(It.IsAny<Guid>()), Times.Once);

        }
#endregion

        #region Native Jobs Controller Tests

        [Fact]
        public void TestNativeJobsControllerGetByIdThrowsUnauthorized()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var nativeJobsController = new NativeJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act amd Assert
            try
            {
                nativeJobsController.Get(jobId);

            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }
        }

        [Fact]
        public void TestNativeJobsControllerGetByIdWithWildcard()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            Guid jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob),
                new Dictionary<string, object> {{"waitForProcess", "false"}, {"consumeStreams", "false"}}, string.Empty,
                jobId);
            

            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            var nativeJobsController = new NativeJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = nativeJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }

        [Fact]
        public void TestNativeJobsControllerGetByIdWithJobGroupName()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NativeJob),
                new Dictionary<string, object> { { "waitForProcess", "false" }, { "consumeStreams", "false" } }, string.Empty,
                jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var nativeJobsController = new NativeJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = nativeJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }
#endregion

        #region Directory Scan Controller Tests

        [Fact]
        public void TestDirectoryScanJobsControllerGetByIdThrowsUnauthorized()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var directoryScanJobsController = new DirectoryScanJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act amd Assert
            try
            {
                directoryScanJobsController.Get(jobId);

            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }
        }

        [Fact]
        public void TestDirectoryScanJobsControllerGetByIdWithWildcard()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            Guid jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob),
                new Dictionary<string, object>(), string.Empty,
                jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            var directoryScanJobsController = new DirectoryScanJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = directoryScanJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }

        [Fact]
        public void TestDirectoryScanJobsControllerGetByIdWithJobGroupName()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob),
                new Dictionary<string, object>(), string.Empty,
                jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var directoryScanJobsController = new DirectoryScanJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = directoryScanJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }
#endregion

        #region Send Email Jobs Controller

        [Fact]
        public void TestSendEmailJobsControllerGetByIdThrowsUnauthorized()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob), new Dictionary<string, object>(), string.Empty, jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "test_group" });
            var sendEmailJobsController = new SendEmailJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act amd Assert
            try
            {
                sendEmailJobsController.Get(jobId);

            }
            catch (HttpResponseException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
            }
        }

        [Fact]
        public void TestSendEmailJobsControllerGetByIdWithWildcard()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            Guid jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NoOpJob),
                new Dictionary<string, object> { { "waitForProcess", "false" }, { "consumeStreams", "false" } }, string.Empty,
                jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "*" });
            var sendEmailJobsController = new SendEmailJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = sendEmailJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }

        [Fact]
        public void TestSendEmailJobsControllerGetByIdWithJobGroupName()
        {
            // Arrange
            IPersistenceStore persistenceStore = new InMemoryStore();
            Scheduler.Shutdown();

            Scheduler.Initialize((config =>
            {
                config.EnableWebApiSelfHost = false;
                config.EnableAuditHistory = false;
                config.PersistenceStoreType = PersistenceStoreType.InMemory;
                config.AutoStart = false;
            }));

            var schedulerCore = new SchedulerCore(Scheduler.Instance(), persistenceStore);

            const string jobName = "Job1";
            const string jobGroup = "Group1";
            var jobId = new Guid("30575FAE-86D3-4EC1-8E10-1E7F5EA6BBB4");

            schedulerCore.CreateJob(jobName, jobGroup, typeof(NativeJob),
                new Dictionary<string, object> { { "waitForProcess", "false" }, { "consumeStreams", "false" } }, string.Empty,
                jobId);


            _mockPermissionsHelper.Setup(i => i.GetAuthorizedJobGroups()).Returns(new List<string> { "Group1" });
            var sendEmailJobsController = new SendEmailJobsController(_mockPermissionsHelper.Object, schedulerCore);

            // Act
            var job = sendEmailJobsController.Get(jobId);

            // Assert
            Assert.Equal(jobGroup, job.JobGroup);
            Assert.Equal(jobName, job.JobName);
        }
#endregion

        
    }
}
