using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insert JobKey and return new (or provided) id.
        /// Return existing id if job key already exists. 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup, Guid? jobId = null)
        {
            Guid retval = jobId == null ? Guid.NewGuid() : jobId.Value;

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

        /// <summary>
        /// Get job id mapped to specified JobKey
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Guid GetJobId(JobKey jobKey)
        {
            Guid retval = Guid.Empty;
            const string cacheKey = "RSCHED_JOB_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                foreach (var mapItem in cacheValue)
                {
                    if (mapItem.Key == jobKey.Name + "|" + jobKey.Group)
                    {
                        retval = mapItem.Value;
                        break;
                    }
                }
            }

            return retval;
        }

        public TriggerKey GetTriggerKey(Guid id)
        {
            TriggerKey retval = null;
            const string cacheKey = "RSCHED_TRIGGER_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                foreach (var mapItem in cacheValue)
                {
                    if (mapItem.Value == id)
                    {
                        retval = new TriggerKey(mapItem.Key.Split('|')[0], mapItem.Key.Split('|')[1]);
                        break;
                    }
                }
            }

            return retval;
        }

        public Guid GetTriggerId(TriggerKey triggerKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insert TriggerKey and return new id.
        /// Return existing id if trigger key already exists. 
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        /// <returns></returns>
        public Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            Guid retval = Guid.NewGuid();

            const string cacheKey = "RSCHED_TRIGGER_ID_KEY_MAP";
            string triggerKey = triggerName + "|" + triggerGroup;

            lock (SyncRoot)
            {
                IDictionary<string, Guid> cacheValue;
                if (Cache.Contains(cacheKey))
                {
                    cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);
                    if (cacheValue.ContainsKey(triggerKey))
                    {
                        retval = cacheValue[triggerKey];
                    }
                    else
                    {
                        cacheValue.Add(triggerKey, retval);
                    }
                }
                else
                {
                    cacheValue = new Dictionary<string, Guid> { { triggerKey, retval } };
                    Cache.Add(cacheKey, cacheValue, ObjectCache.InfiniteAbsoluteExpiration);
                }
            }

            return retval;
        }

        public void RemoveTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertCalendarIdMap(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get calendar name mapped to specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetCalendarName(Guid id)
        {
            string retval = null;
            const string cacheKey = "RSCHED_CALENDAR_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                foreach (var mapItem in cacheValue)
                {
                    if (mapItem.Value == id)
                    {
                        retval = mapItem.Key;
                        break;
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// Delete Calendar id mapping
        /// </summary>
        /// <param name="name"></param>
        public void RemoveCalendarIdMap(string name)
        {
            const string cacheKey = "RSCHED_CALENDAR_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                string keyToRemove = (from mapItem in cacheValue where mapItem.Key == name select mapItem.Key).FirstOrDefault();

                if (!string.IsNullOrEmpty(keyToRemove))
                {
                    cacheValue.Remove(keyToRemove);
                }
            }
        }

        /// <summary>
        /// Get calendar id mapped to specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid GetCalendarId(string name)
        {
            Guid retval = Guid.Empty;
            const string cacheKey = "RSCHED_CALENDAR_ID_KEY_MAP";

            if (Cache.Contains(cacheKey))
            {
                var cacheValue = (IDictionary<string, Guid>)Cache.Get(cacheKey);

                foreach (var mapItem in cacheValue)
                {
                    if (mapItem.Key == name)
                    {
                        retval = mapItem.Value;
                        break;
                    }
                }
            }

            return retval;
        }
    }
}
