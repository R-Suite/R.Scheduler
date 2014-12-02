using System;
using System.Reflection;
using Common.Logging;
using Newtonsoft.Json;
using Quartz;
using Quartz.Spi;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    /// <summary>
    /// Persist history of all job executions via implementation of <see cref="IPersistanceStore"/> 
    /// configured during the Scheduler initialisation.
    /// Log history of all job executions via <see cref="Common.Logging"/>.
    /// </summary>
    public class AuditJobListener : IJobListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPersistanceStore _persistanceStore;

        public string Name
        {
            get { return "AuditJobListener"; }
        }

        public AuditJobListener()
        {
            _persistanceStore = ObjectFactory.GetInstance<IPersistanceStore>();
        }

        /// <summary>
        /// Called by the Scheduler when a <see cref="IJobDetail"/> is
        /// about to be executed (an associated <see cref="ITrigger"/> has occurred). 
        /// This method will not be invoked if the execution of the Job was vetoed by a <see cref="ITriggerListener"/>.
        /// </summary>
        /// <seealso cref="JobExecutionVetoed(IJobExecutionContext)"/>
        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Logger.InfoFormat("JobToBeExecuted: {0}, {1}", context.JobDetail.Key.Name, context.JobDetail.Key.Group);

            var auditLog = GetAuditLog("JobToBeExecuted", context);
            _persistanceStore.InsertAuditLog(auditLog);
        }

        /// <summary>
        /// Called by the Schedule when a <see cref="IJobDetail" />
        /// was about to be executed (an associated <see cref="ITrigger" />
        /// has occured), but a <see cref="ITriggerListener" /> vetoed it's
        /// execution.
        /// </summary>
        /// <param name="context"></param>
        /// <seealso cref="JobToBeExecuted(IJobExecutionContext)"/>
        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            Logger.InfoFormat("JobExecutionVetoed: {0}, {1}", context.JobDetail.Key.Name, context.JobDetail.Key.Group);

            var auditLog = GetAuditLog("JobExecutionVetoed", context);
            _persistanceStore.InsertAuditLog(auditLog);
        }

        /// <summary>
        /// Called by the Scheduler after a <see cref="IJobDetail" />
        /// has been executed, and be for the associated <see cref="ITrigger" />'s
        /// <see cref="IOperableTrigger.Triggered" /> method has been called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Logger.InfoFormat("JobWasExecuted: {0}, {1}", context.JobDetail.Key.Name, context.JobDetail.Key.Group);

            var auditLog = GetAuditLog("JobWasExecuted", context);
            if (null != jobException)
            {
                auditLog.ExecutionException = jobException.ToString();
            }
            
            _persistanceStore.InsertAuditLog(auditLog);
        }

        private static AuditLog GetAuditLog(string action, IJobExecutionContext context)
        {
            var trigger = context.Trigger;
            var auditLog = new AuditLog
            {
                Action = action,
                TimeStamp = DateTime.UtcNow,
                FireInstanceId = context.FireInstanceId,
                FireTimeUtc = context.FireTimeUtc,
                ScheduledFireTimeUtc = context.ScheduledFireTimeUtc,
                JobName = context.JobDetail.Key.Name,
                JobGroup = context.JobDetail.Key.Group,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                JobRunTime = context.JobRunTime,
                JobType = context.JobDetail.JobType.Name,
                Params = JsonConvert.SerializeObject(context.MergedJobDataMap),
                RefireCount = context.RefireCount,
                Recovering = context.Recovering,
                Result = (context.Result != null) ? context.Result.ToString() : null
            };

            return auditLog;
        }
    }
}