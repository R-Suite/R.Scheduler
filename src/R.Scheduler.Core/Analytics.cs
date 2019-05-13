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
        /// <returns></returns>
        public IEnumerable<FireInstance> GetExecutingJobs()
        {
            IList<FireInstance> retval = new List<FireInstance>();

            var executingJobs = _scheduler.GetCurrentlyExecutingJobs();

            foreach (var executingJob in executingJobs)
            {
                retval.Add(new FireInstance
                {
                    FireTimeUtc = executingJob.Trigger.GetPreviousFireTimeUtc(),
                    JobName = executingJob.JobDetail.Key.Name,
                    JobGroup = executingJob.JobDetail.Key.Group,
                    TriggerName = executingJob.Trigger.Key.Name,
                    TriggerGroup = executingJob.Trigger.Key.Group,
                    JobId = _persistanceStore.GetJobId(executingJob.JobDetail.Key)
                });
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

        /// <summary>
        /// Get a specified number of upcoming jobs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<FireInstance> GetUpcomingJobs(int count)
        {
            IList<FireInstance> temp = new List<FireInstance>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            try
            {
                foreach (var triggerKey in allTriggerKeys)
                {
                    ITrigger trigger = _scheduler.GetTrigger(triggerKey);

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
                                JobId = _persistanceStore.GetJobId(trigger.JobKey)
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
        /// <returns></returns>
        public IEnumerable<FireInstance> GetUpcomingJobsBetween(DateTime from, DateTime to)
        {
            IList<FireInstance> temp = new List<FireInstance>();

            var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            try
            {
                foreach (var triggerKey in allTriggerKeys)
                {
                    ITrigger trigger = _scheduler.GetTrigger(triggerKey);

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
                                JobId = _persistanceStore.GetJobId(trigger.JobKey)
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
        public IEnumerable<AuditLog> GetJobExecutionsBetween(Guid id, DateTime from, DateTime to)
        {
            return _persistanceStore.GetJobExecutionsBetween(id, from, to);
        }
    }
}
