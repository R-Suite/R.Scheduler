using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// InMemory implementation of ICustomJobStore
    /// </summary>
    public class InMemoryStore : ICustomJobStore
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
        private CacheItemPolicy _policy; 

        /// <summary>
        /// Get registered custom job
        /// </summary>
        /// <param name="name">job name</param>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public ICustomJob GetRegisteredJob(string name, string jobType)
        {
            ICustomJob retval = null;

            foreach (KeyValuePair<string, object> caheItem in Cache)
            {
                if (((ICustomJob) caheItem.Value).Name == name && ((ICustomJob) caheItem.Value).JobType == jobType)
                {
                    retval = (ICustomJob) caheItem.Value;
                    break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Get registered custom job
        /// </summary>
        /// <param name="id">job id</param>
        /// <returns></returns>
        public ICustomJob GetRegisteredJob(Guid id)
        {
            return Cache.Select(caheItem => (ICustomJob)caheItem.Value).FirstOrDefault(plugin => id == plugin.Id);
        }

        /// <summary>
        /// Get all registered jobs of type <paramref name="jobType"/>
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public IList<ICustomJob> GetRegisteredJobs(string jobType)
        {
            IList<ICustomJob> retval = new List<ICustomJob>();

            if (Cache.GetCount() > 0)
            {
                foreach (KeyValuePair<string, object> caheItem in Cache)
                {
                    if (((ICustomJob)caheItem.Value).JobType == jobType || string.IsNullOrEmpty(jobType))
                        retval.Add((ICustomJob)caheItem.Value);
                }
            }

            return retval;
        }

        /// <summary>
        /// Register new job, or update existing one.
        /// </summary>
        /// <param name="job"></param>
        public void RegisterJob(ICustomJob job)
        {
            if (string.IsNullOrEmpty(job.JobType))
            {
                throw new Exception("JobType not specified.");
            }

            _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(10.00) };

            foreach (KeyValuePair<string, object> caheItem in Cache)
            {
                if (((ICustomJob) caheItem.Value).JobType == job.JobType && ((ICustomJob) caheItem.Value).Name == job.Name)
                {
                    job.Id = ((ICustomJob) caheItem.Value).Id;
                    break;
                }
            }

            if (job.Id == Guid.Empty)
            {
                job.Id = Guid.NewGuid();
            }

            Cache.Set(job.Id.ToString(), job, _policy);
        }

        /// <summary>
        /// Update job name
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void UpdateName(Guid id, string name)
        {
            if (Cache.Contains(id.ToString()))
            {
                var job = (ICustomJob)Cache[id.ToString()];
                job.Name = name;
            }
            else
            {
                throw new Exception(string.Format("{0} not found.", name));
            }
        }

        /// <summary>
        /// Remove job from cache object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Remove(Guid id)
        {
            if (Cache.Contains(id.ToString()))
            {
                Cache.Remove(id.ToString());

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Remove all registered jobs of type <paramref name="jobType"/>
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public int RemoveAll(string jobType)
        {
            if (Cache.GetCount() > 0)
            {
                int count = 0;
                foreach (KeyValuePair<string, object> caheItem in Cache)
                {
                    if (((ICustomJob) caheItem.Value).JobType == jobType)
                    {
                        Cache.Remove(caheItem.Key);
                        count++;
                    }
                }

                return count;
            }

            return 0;
        }
    }
}
