using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Controllers
{
    public class TriggerHelper
    {
        public static IList<TriggerDetails> GetTriggerDetails(IEnumerable<KeyValuePair<ITrigger, Guid>> quartzTriggers)
        {
            IList<TriggerDetails> triggerDetails = new List<TriggerDetails>();

            foreach (KeyValuePair<ITrigger, Guid> trigger in quartzTriggers)
            {
                ITrigger quartzTrigger = trigger.Key;
                var triggerType = "InstructionNotSet";
                var misfireInstruction = string.Empty;
                var additionalDetails = string.Empty;
                if (quartzTrigger is ICronTrigger)
                {
                    triggerType = "Cron";
                    switch (quartzTrigger.MisfireInstruction)
                    {
                        case 0:
                            misfireInstruction = "SmartPolicy";
                            break;
                        case 1:
                            misfireInstruction = "FireOnceNow";
                            break;
                        case 2:
                            misfireInstruction = "DoNothing";
                            break;
                        case -1:
                            misfireInstruction = "IgnoreMisfirePolicy";
                            break;
                    }
                    additionalDetails = string.Format("Cron Expression: {0}", ((ICronTrigger)quartzTrigger).CronExpressionString);
                }
                if (quartzTrigger is ISimpleTrigger)
                {
                    triggerType = "Simple";
                    switch (quartzTrigger.MisfireInstruction)
                    {
                        case 0:
                            misfireInstruction = "SmartPolicy";
                            break;
                        case 1:
                            misfireInstruction = "FireNow";
                            break;
                        case 2:
                            misfireInstruction = "RescheduleNowWithExistingRepeatCount";
                            break;
                        case 3:
                            misfireInstruction = "RescheduleNowWithRemainingRepeatCount";
                            break;
                        case 4:
                            misfireInstruction = "RescheduleNextWithRemainingCount";
                            break;
                        case 5:
                            misfireInstruction = "RescheduleNextWithExistingCount";
                            break;
                        case -1:
                            misfireInstruction = "IgnoreMisfirePolicy";
                            break;
                    }
                    additionalDetails = string.Format("Repeat Interval: {0}. Repeat Count: {1}", ((ISimpleTrigger)quartzTrigger).RepeatInterval, ((ISimpleTrigger)quartzTrigger).RepeatCount);
                }
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();

                var jobDataMap = new Dictionary<string, object>();
                if (null != quartzTrigger.JobDataMap)
                {
                    foreach (var jobData in quartzTrigger.JobDataMap)
                    {
                        jobDataMap.Add(jobData.Key, jobData.Value);
                    }
                }

                triggerDetails.Add(new TriggerDetails
                {
                    Id = trigger.Value,
                    Name = quartzTrigger.Key.Name,
                    Group = quartzTrigger.Key.Group,
                    JobName = quartzTrigger.JobKey.Name,
                    JobGroup = quartzTrigger.JobKey.Group,
                    Description = quartzTrigger.Description,
                    CalendarName = quartzTrigger.CalendarName,
                    StartTimeUtc = quartzTrigger.StartTimeUtc.UtcDateTime,
                    EndTimeUtc =
                        (quartzTrigger.EndTimeUtc.HasValue)
                            ? quartzTrigger.EndTimeUtc.Value.UtcDateTime
                            : (DateTime?) null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?) null,
                    PreviousFireTimeUtc =
                        (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?) null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue)
                        ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime
                        : (DateTime?) null,
                    Type = triggerType,
                    MisfireInstruction = misfireInstruction,
                    AdditionalDetails = additionalDetails,
                    JobDataMap = (jobDataMap.Count > 0) ? jobDataMap : null
                });
            }
            return triggerDetails;
        }
    }
}