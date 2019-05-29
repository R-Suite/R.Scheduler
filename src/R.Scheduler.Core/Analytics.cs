using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Provides analytical data about scheduled jobs
    /// </summary>
    public class Analytics : IAnalytics
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IScheduler _scheduler;
        private readonly IPersistenceStore _persistenceStore;

        public Analytics(IScheduler scheduler, IPersistenceStore persistenceStore)
        {
            _scheduler = scheduler;
            _persistenceStore = persistenceStore;
        }

        /// <summary>
        /// Get number of job setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetJobCount()
        {
            return _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Count;
        }

        /// <summary>
        /// Get number of triggers setup in scheduler
        /// </summary>
        /// <returns></returns>
        public int GetTriggerCount()
        {
            return _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).Count;
        }

        /// <summary>
        /// Get fire instances of currently executing jobs.
        /// </summary>
        /// <param name="authorizedJobGroups"></param>
        /// <returns></returns>
        public IEnumerable<FireInstance> GetExecutingJobs(IEnumerable<string> authorizedJobGroups)
        {
            IList<FireInstance> retval = new List<FireInstance>();

            var executingJobs = _scheduler.GetCurrentlyExecutingJobs();
            var jobGroups = authorizedJobGroups.ToList();

            foreach (var executingJob in executingJobs)
            {
                if (!jobGroups.Contains(executingJob.JobDetail.Key.Group)) continue;

                retval.Add(new FireInstance
                {
                    FireTimeUtc = executingJob.Trigger.GetPreviousFireTimeUtc(),
                    JobName = executingJob.JobDetail.Key.Name,
                    JobGroup = executingJob.JobDetail.Key.Group,
                    TriggerName = executingJob.Trigger.Key.Name,
                    TriggerGroup = executingJob.Trigger.Key.Group,
                    JobId = _persistenceStore.GetJobId(executingJob.JobDetail.Key)
                });
            }

            return retval;
        }

        /// <summary>
        /// Get a specified number of most recently failed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <param name="authorizedJobGroups"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetErroredJobs(int count, IEnumerable<string> authorizedJobGroups)
        {
            var erroredJobs = _persistenceStore.GetErroredJobs(count);

            var jobGroups = authorizedJobGroups.ToList();
            if (jobGroups.Contains("*")) return erroredJobs;

            return erroredJobs.Where(jobExecution => jobGroups.Contains(jobExecution.JobGroup)).ToList();
        }

        /// <summary>
        /// Get a specified number of most recently executed jobs
        /// </summary>
        /// <param name="count"></param>
        /// <param name="authorizedJobGroups"></param>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetExecutedJobs(int count, IEnumerable<string> authorizedJobGroups)
        {
            var executedJobs = _persistenceStore.GetExecutedJobs(count);

            var jobGroups = authorizedJobGroups.ToList();
            if (jobGroups.Contains("*")) return executedJobs;

            return executedJobs.Where(jobExecution => jobGroups.Contains(jobExecution.JobGroup)).ToList();
        }

        /// <summary>
        /// Get a specified number of upcoming jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<FireInstance> GetUpcomingJobs(int count, IEnumerable<string> authorizedJobGroups)
        {
            IList<FireInstance> temp = new List<FireInstance>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            try
            {
                foreach (var triggerKey in allTriggerKeys)
                {
                    ITrigger trigger = _scheduler.GetTrigger(triggerKey);

                    if (authorizedJobGroups != null && !authorizedJobGroups.Contains(trigger.JobKey.Group) &&
                        !authorizedJobGroups.Contains("*")) continue;

                    ICalendar cal = null;
                    if (!string.IsNullOrEmpty(trigger.CalendarName))
                    {
                        cal = _scheduler.GetCalendar(trigger.CalendarName);
                    }
                    var fireTimes = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, cal, count);
                    //TriggerUtils.ComputeFireTimesBetween(trigger as IOperableTrigger, cal,)

                    foreach (var dateTimeOffset in fireTimes)
                    {
                        if (dateTimeOffset > DateTime.UtcNow) // Paused triggers might have the next firetime in the past.
                        {
                            temp.Add(new FireInstance
                            {
                                FireTimeUtc = dateTimeOffset,
                                JobName = trigger.JobKey.Name,
                                JobGroup = trigger.JobKey.Group,
                                TriggerName = trigger.Key.Name,
                                TriggerGroup = trigger.Key.Group,
                                JobId = _persistenceStore.GetJobId(trigger.JobKey)
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error getting upcoming job", e);
                throw;
            }

            IList<FireInstance> retval = temp.OrderBy(i => i.FireTimeUtc).Take(count).ToList();

            return retval;
        }


        /// <summary>
        /// Get a specified number of upcoming jobs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="authorizedJobGroups"></param>
        /// <returns></returns>
        public IEnumerable<FireInstance> GetUpcomingJobsBetween(DateTime from, DateTime to, IEnumerable<string> authorizedJobGroups)
        {
            IList<FireInstance> temp = new List<FireInstance>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            try
            {
                foreach (var triggerKey in allTriggerKeys)
                {
                    ITrigger trigger = _scheduler.GetTrigger(triggerKey);

                    if (authorizedJobGroups != null && !authorizedJobGroups.Contains(trigger.JobKey.Group) &&
                        !authorizedJobGroups.Contains("*")) continue;

                    ICalendar cal = null;
                    if (!string.IsNullOrEmpty(trigger.CalendarName))
                    {
                        cal = _scheduler.GetCalendar(trigger.CalendarName);
                    }
                    var fireTimes = TriggerUtils.ComputeFireTimesBetween(trigger as IOperableTrigger, cal, from, to);

                    foreach (var dateTimeOffset in fireTimes)
                    {
                        if (dateTimeOffset > DateTime.UtcNow) // Paused triggers might have the next firetime in the past.
                        {
                            temp.Add(new FireInstance
                            {
                                FireTimeUtc = dateTimeOffset,
                                JobName = trigger.JobKey.Name,
                                JobGroup = trigger.JobKey.Group,
                                TriggerName = trigger.Key.Name,
                                TriggerGroup = trigger.Key.Group,
                                JobId = _persistenceStore.GetJobId(trigger.JobKey)
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error getting upcoming job", e);
                throw;
            }

            IList<FireInstance> retval = temp.OrderBy(i => i.FireTimeUtc).ToList();

            return retval;
        }


        /// <summary>
        /// Get executions for a specified job
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to, IEnumerable<string> authorizedJobGroups)
        {
            var jobExecutions = _persistenceStore.GetJobExecutionsBetween(id, from, to);

            var jobGroups = authorizedJobGroups.ToList();
            if (jobGroups.Contains("*")) return jobExecutions;

            return jobExecutions.Where(jobExecution => jobGroups.Contains(jobExecution.JobGroup)).ToList();
        }
    }
}
