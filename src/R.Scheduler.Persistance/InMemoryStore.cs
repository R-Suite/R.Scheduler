using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// InMemory implementation of IPersistanceStore
    /// </summary>
    public class InMemoryStore : IPersistanceStore
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private CacheItemPolicy _policy;

        public InMemoryStore()
        {
            _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(10.00) };
        }

        public void InsertAuditLog(AuditLog log)
        {
            _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(10.00) };

            if (log.TimeStamp == DateTime.MinValue)
            {
                log.TimeStamp = DateTime.UtcNow;
            }

            Cache.Set(log.TimeStamp.ToString(CultureInfo.InvariantCulture), log, _policy);
        }

        public int GetJobDetailsCount()
        {
            throw new NotImplementedException();
        }

        public int GetTriggerCount()
        {
            throw new NotImplementedException();
        }

        public IList<TriggerKey> GetFiredTriggers()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup)
        {
            throw new NotImplementedException();
        }

        public JobKey GetJobKey(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid GetJobId(JobKey jobKey)
        {
            throw new NotImplementedException();
        }

        public TriggerKey GetTriggerKey(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertCalendarIdMap(string name)
        {
            throw new NotImplementedException();
        }

        public string GetCalendarName(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
