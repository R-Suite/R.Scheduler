using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Quartz;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Provides analytical data about scheduled jobs
    /// </summary>
    public class Analytics : IAnalytics
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IScheduler _scheduler;
        private readonly IPersistanceStore _persistanceStore;

        public Analytics(IScheduler scheduler)
        {
            _scheduler = scheduler;
            _persistanceStore = ObjectFactory.GetInstance<IPersistanceStore>();
            ObjectFactory.GetInstance<ISchedulerCore>();
        }

        public int GetJobCount()
        {
            return _persistanceStore.GetJobDetailsCount();
        }

        public int GetTriggerCount()
        {
            return _persistanceStore.GetTriggerCount();
        }

        public IEnumerable<ITrigger> GetFiredTriggers()
        {
            IList<ITrigger> retval = new List<ITrigger>();
            var firedTriggers = _persistanceStore.GetFiredTriggers();

            foreach (var firedTrigger in firedTriggers)
            {
                retval.Add(_scheduler.GetTrigger(firedTrigger));
            }

            return retval;
        }

        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            return _persistanceStore.GetErroredJobs(count);
        }
    }
}
