using System;

namespace R.Scheduler.Interfaces
{
    public interface ICustomJob
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Params { get; set; }
        string JobType { get; set; }
    }
}
