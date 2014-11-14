using System;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using Xunit;

namespace R.Scheduler.UnitTests
{
    public class TestCustomJob : ICustomJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Params { get; set; }
        public string JobType { get; set; }
    }

    public class InMemoryStoreTests
    {
        [Fact]
        public void ShouldGetRegisteredCustomJobByName()
        {
            // Arrange
            var cutomJob = new TestCustomJob { Name = "name1", Params = "param1", JobType = "jobtype1"};
            ICustomJobStore store = new InMemoryStore();
            store.RemoveAll("jobtype1");
            store.RegisterJob(cutomJob);

            // Act 
            var result = store.GetRegisteredJob("name1", "jobtype1");

            // Assert
            Assert.Equal("param1", result.Params);
        }

        [Fact]
        public void ShouldThrowWhenRegisteringCustomJobWithoutType()
        {
            // Arrange
            var cutomJob = new TestCustomJob { Name = "name1", Params = "param1" };
            ICustomJobStore store = new InMemoryStore();

            // Act / Assert 
            Assert.Throws<Exception>(() => store.RegisterJob(cutomJob));
        }

        [Fact]
        public void ShouldGetRegisteredCustomJobById()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var customJob = new TestCustomJob { Id = id, Name = "name1", Params = "param1", JobType = "jobtype1" };
            ICustomJobStore store = new InMemoryStore();
            store.RemoveAll("jobtype1");
            store.RegisterJob(customJob);

            // Act 
            var result = store.GetRegisteredJob(id);

            // Assert
            Assert.Equal("param1", result.Params);
            Assert.Equal("jobtype1", result.JobType);
        }

        [Fact]
        public void ShouldReturnNullWhenGettingUnRegisteredCustomJob()
        {
            // Arrange
            ICustomJobStore store = new InMemoryStore();
            store.RemoveAll("jobtype1");

            // Act 
            var result = store.GetRegisteredJob("APlugin", "jobtype1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ShouldUpdatePreviouslyRegisteredCustomJob()
        {
            // Arrange
            var customJob = new TestCustomJob { Name = "name", Params = "param1", JobType = "jobtype1" };
            var customJob2 = new TestCustomJob { Name = "name", Params = "param2", JobType = "jobtype1" };
            ICustomJobStore store = new InMemoryStore();
            store.RegisterJob(customJob);

            // Act 
            store.RegisterJob(customJob2);
            var result = store.GetRegisteredJob("name", "jobtype1");

            // Assert
            Assert.Equal("param2", result.Params);
        }

        [Fact]
        public void ShouldGetAllRegisteredCustomJobs()
        {
            // Arrange
            var customJob = new TestCustomJob { Name = "name", Params = "param1", JobType = "jobtype1" };
            var customJob2 = new TestCustomJob { Name = "name2", Params = "param2", JobType = "jobtype1" };
            ICustomJobStore store = new InMemoryStore();
            store.RemoveAll("jobtype1");
            store.RegisterJob(customJob);
            store.RegisterJob(customJob2);

            // Act 
            var result = store.GetRegisteredJobs("jobtype1");

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ShouldRemoveCustomJob()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            var customJob = new TestCustomJob { Id = id, Name = "name", Params = "param1", JobType = "jobtype1" };
            var customJob2 = new TestCustomJob { Id = id2, Name = "name2", Params = "param2", JobType = "jobtype1" };
            ICustomJobStore store = new InMemoryStore();
            store.RemoveAll("jobtype1");
            store.RegisterJob(customJob);
            store.RegisterJob(customJob2);

            // Act 
            var result = store.Remove(id);

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(1, store.GetRegisteredJobs("jobtype1").Count);
        }
    }
}
