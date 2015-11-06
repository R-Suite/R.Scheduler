using System.Collections.Generic;
using Quartz;

namespace R.Scheduler.Interfaces
{
    public interface IAnalytics
    {
        int GetJobCount();
        int GetTriggerCount();
        IEnumerable<ITrigger> GetFiredTriggers();
    }
}
