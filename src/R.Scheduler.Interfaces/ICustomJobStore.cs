using System;
using System.Collections.Generic;

namespace R.Scheduler.Interfaces
{
    public interface ICustomJobStore
    {
        ICustomJob GetRegisteredJob(string name, string jobType);

        ICustomJob GetRegisteredJob(Guid id);

        IList<ICustomJob> GetRegisteredJobs(string jobType);

        void RegisterJob(ICustomJob job);

        void UpdateName(Guid id, string name);

        int Remove(Guid id);

        int RemoveAll(string jobType);
    }
}
