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
    /// Persist history of all trigger firings via implementation of <see cref="IPersistanceStore"/> 
    /// configured during the Scheduler initialisation.
    /// Log history of all trigger firings via <see cref="Common.Logging"/>.
    /// </summary>
    public class AuditTriggerListener : ITriggerListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistanceStore _persistanceStore;

        public string Name
        {
            get { return "AuditTriggerListener"; }
        }

        public AuditTriggerListener(IPersistanceStore persistancestore)
        {
            _persistanceStore = persistancestore;
        }

        /// <summary>
        /// Called by the Scheduler when a <see cref="ITrigger" /> has fired, 
        /// and it's associated JobDetail is about to be executed.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Logger.InfoFormat("TriggerFired: {0}, {1}", trigger.Key.Name, trigger.Key.Group);

            var auditLog = GetAuditLog(trigger, "TriggerFired", context);
            _persistanceStore.InsertAuditLog(auditLog);
        }

        /// <summary>
        /// If the implementation vetos the execution (via returning true, the job's execute method will not be called.
        /// <see cref="AuditTriggerListener"/> implementation does not veto execution.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <returns>false</returns>
        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            // todo: might want to veto if the same job (triggered previously) is still executing
            return false;
        }

        /// <summary>
        /// Called by the Scheduler when a <see cref="ITrigger" /> has misfired.
        /// </summary>
        /// <param name="trigger"></param>
        public void TriggerMisfired(ITrigger trigger)
        {
            Logger.InfoFormat("TriggerMisfired: {0}, {1}", trigger.Key.Name, trigger.Key.Group);

            var auditLog = GetAuditLog(trigger, "TriggerMisfired");
            _persistanceStore.InsertAuditLog(auditLog);
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
        /// has fired, it's associated <see cref="IJobDetail" />
        /// has been executed, and it's <see cref="IOperableTrigger.Triggered" /> method has been
        /// called.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="triggerInstructionCode"></param>
        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            Logger.InfoFormat("TriggerComplete: {0}, {1}", trigger.Key.Name, trigger.Key.Group);

            var auditLog = GetAuditLog(trigger, "TriggerComplete", context);
            _persistanceStore.InsertAuditLog(auditLog);
        }

        private static AuditLog GetAuditLog(ITrigger trigger, string action, IJobExecutionContext context = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                TimeStamp = DateTime.UtcNow,
                JobName = trigger.JobKey.Name,
                JobGroup = trigger.JobKey.Group,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
            };

            if (context != null)
            {
                auditLog.FireInstanceId = context.FireInstanceId;
                auditLog.FireTimeUtc = context.FireTimeUtc;
                auditLog.FireTimeUtc = context.FireTimeUtc;
                auditLog.ScheduledFireTimeUtc = context.ScheduledFireTimeUtc;
                auditLog.JobRunTime = context.JobRunTime;
                auditLog.JobType = context.JobDetail.JobType.Name;
                auditLog.Params = JsonConvert.SerializeObject(context.MergedJobDataMap);
                auditLog.RefireCount = context.RefireCount;
                auditLog.Recovering = context.Recovering;
                auditLog.Result = (context.Result != null) ? context.Result.ToString() : null;
            }

            return auditLog;
        }
    }
}
