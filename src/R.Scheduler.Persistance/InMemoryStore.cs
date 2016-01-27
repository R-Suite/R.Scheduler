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
        private static readonly object SyncRoot = new Object();

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

        /// <summary>
        /// Insert JobKey and return new id.
        /// Return existing id if job key already exists. 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup)
        {
            Guid retval = Guid.NewGuid();

            const string cacheKey = "RSCHED_JOB_ID_KEY_MAP";
            string jobKey = jobName + "|" + jobGroup;

            lock (SyncRoot)
            {
                IDictionary<string, Guid> cacheValue;
                if (Cache.Contains(cacheKey))
                {
                    cacheValue = (IDictionary<string, Guid>) Cache.Get(cacheKey);
                    if (cacheValue.ContainsKey(jobKey))
                    {
                        retval = cacheValue[jobKey];
                    }
                    else
                    {
                        cacheValue.Add(jobKey, retval);
                    }
                }
                else
                {
                    cacheValue = new Dictionary<string, Guid> {{jobKey, retval}};
                    Cache.Add(cacheKey, cacheValue, ObjectCache.InfiniteAbsoluteExpiration);
                }
            }

            return retval;
        }

        public void RemoveJobKeyIdMap(string jobName, string jobGroup)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get JobKey mapped to specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JobKey GetJobKey(Guid id)
        {
            JobKey retval = null;
            const string cacheKey = "RSCHED_JOB_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                foreach (var mapItem in cacheValue)
                {
                    if (mapItem.Value == id)
                    {
                        retval = new JobKey(mapItem.Key.Split('|')[0], mapItem.Key.Split('|')[1]);
                        break;
                    }
                }
            }

            return retval;
        }

        public Guid GetJobId(JobKey jobKey)
        {
            throw new NotImplementedException();
        }

        public TriggerKey GetTriggerKey(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid GetTriggerId(TriggerKey triggerKey)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public void RemoveTriggerKeyIdMap(string triggerName, string triggerGroup)
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
