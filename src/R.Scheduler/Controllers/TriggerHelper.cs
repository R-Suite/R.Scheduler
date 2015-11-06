using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Controllers
{
    public class TriggerHelper
    {
        public static IList<TriggerDetails> GetTriggerDetails(IEnumerable<ITrigger> quartzTriggers)
        {
            IList<TriggerDetails> triggerDetails = new List<TriggerDetails>();

            foreach (ITrigger quartzTrigger in quartzTriggers)
            {
                var triggerType = "InstructionNotSet";
                var misfireInstruction = string.Empty;
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
                }
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();
                triggerDetails.Add(new TriggerDetails
                {
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
                            : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc =
                        (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue)
                        ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime
                        : (DateTime?)null,
                    Type = triggerType,
                    MisfireInstruction = misfireInstruction
                });
            }
            return triggerDetails;
        }
    }
}