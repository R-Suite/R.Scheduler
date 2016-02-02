using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Provides analytical data about scheduled jobs
    /// </summary>
    public class Analytics : IAnalytics
    {
        private readonly IScheduler _scheduler;
        private readonly IPersistanceStore _persistanceStore;

        public Analytics(IScheduler scheduler, IPersistanceStore persistanceStore)
        {
            _scheduler = scheduler;
            _persistanceStore = persistanceStore;
        }

        /// <summary>
        /// Get number of job setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetJobCount()
        {
            return _persistanceStore.GetJobDetailsCount();
        }

        /// <summary>
        /// Get number of triggers setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetTriggerCount()
        {
            return _persistanceStore.GetTriggerCount();
        }

        /// <summary>
        /// Get currently executing triggers mapped to trigger ids
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<ITrigger, Guid>> GetFiredTriggers()
        {
            IDictionary<ITrigger, Guid> retval = new Dictionary<ITrigger, Guid>();
            IEnumerable<TriggerKey> firedTriggers = _persistanceStore.GetFiredTriggers();

            foreach (var firedTrigger in firedTriggers)
            {
                var triggerId = _persistanceStore.GetTriggerId(firedTrigger);
                retval.Add(_scheduler.GetTrigger(firedTrigger), triggerId);
            }

            return retval;
        }

        /// <summary>
        /// Get a specified number of most recently failed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            return _persistanceStore.GetErroredJobs(count);
        }

        /// <summary>
        /// Get a specified number of most recently executed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            return _persistanceStore.GetExecutedJobs(count);
        }
    }
}
