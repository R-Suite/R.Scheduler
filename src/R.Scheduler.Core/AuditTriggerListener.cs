using System;
using System.Reflection;
using Common.Logging;
using Newtonsoft.Json;
using Quartz;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    public class AuditTriggerListener : ITriggerListener 
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistanceStore _persistanceStore;
        private readonly ISchedulerCore _schedulerCore;

        public AuditTriggerListener()
        {
            _persistanceStore = ObjectFactory.GetInstance<IPersistanceStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            var auditLog = GetAuditLog(trigger, context, "TriggerFired");
            _persistanceStore.InsertAuditLog(auditLog);
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            var jobDetail = _schedulerCore.GetJobDetail(trigger.JobKey.Name, trigger.JobKey.Group);
            var auditLog = new AuditLog
            {
                Action = "TriggerMisfired",
                TimeStamp = DateTime.UtcNow,
                JobName = trigger.JobKey.Group,
                JobGroup = trigger.JobKey.Group,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                JobType = jobDetail.JobType.Name,
                Params = JsonConvert.SerializeObject(trigger.JobDataMap),
            };
            _persistanceStore.InsertAuditLog(auditLog);
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            var auditLog = GetAuditLog(trigger, context, "TriggerComplete");
            _persistanceStore.InsertAuditLog(auditLog);
        }

        public string Name
        {
            get { return "AuditTriggerListener"; }
        }

        private static AuditLog GetAuditLog(ITrigger trigger, IJobExecutionContext context, string action)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                TimeStamp = DateTime.UtcNow,
                FireInstanceId = context.FireInstanceId,
                FireTimeUtc = context.FireTimeUtc,
                ScheduledFireTimeUtc = context.ScheduledFireTimeUtc,
                JobName = trigger.JobKey.Group,
                JobGroup = trigger.JobKey.Group,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                JobRunTime = context.JobRunTime,
                JobType = context.JobDetail.JobType.Name,
                Params = JsonConvert.SerializeObject(context.MergedJobDataMap),
                RefireCount = context.RefireCount,
                Recovering = context.Recovering,
                Result = context.Result.ToString()
            };

            return auditLog;
        }
    }
}
