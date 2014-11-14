using System;

namespace R.Scheduler.Interfaces
{
    public interface IJobTypeManager
    {
        void Register(string name, params string[] args);

        void Remove(Guid id);
    }
}
