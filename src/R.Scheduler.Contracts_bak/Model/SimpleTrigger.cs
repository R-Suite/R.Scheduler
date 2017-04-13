using System;

namespace R.Scheduler.Contracts.Model
{
    public enum MisfireInstructionSimple
    {
        /// <summary>
        /// If the Repeat Count is 0, then the instruction will be interpreted as FireNow. 
        /// If the Repeat Count is RepeatIndefinitely, then the instruction will be interpreted as RescheduleNowWithRemainingRepeatCount. 
        /// </summary>
        SmartPolicy,
        /// <summary>
        /// Used for 'one-shot' (non-repeating) Triggers
        /// </summary>
        FireNow,
        /// <summary>
        /// If a trigger has missed several of its scheduled firings, 
        /// then several rapid firings may occur as the trigger attempt to catch back up to where it would have been. 
        /// For example, a SimpleTrigger that fires every 15 seconds which has misfired for 5 minutes 
        /// will fire 20 times once it gets the chance to fire.
        /// </summary>
        Ignore,
        /// <summary>
        /// Instructs the IScheduler that upon a mis-fire situation, 
        /// the ISimpleTrigger wants to be re-scheduled to the next scheduled time after 'now' - taking into account any associated Calendar, 
        /// and with the repeat count left unchanged.
        /// This instruction could cause the ITrigger to go directly to the 'COMPLETE' state if all the end-time of the trigger has arrived.
        /// </summary>
        RescheduleNextWithExistingCount,
        /// <summary>
        /// Instructs the Scheduler that upon a mis-fire situation, 
        /// the SimpleTrigger wants to be re-scheduled to the next scheduled time after 'now' - taking into account any associated Calendar, 
        /// and with the repeat count set to what it would be, if it had not missed any firings.
        /// </summary>
        RescheduleNextWithRemainingCount,
        /// <summary>
        /// Instructs the Scheduler that upon a mis-fire situation, 
        /// the SimpleTrigger wants to be re-scheduled to 'now' (even if the associated ICalendar excludes 'now') 
        /// with the repeat count left as-is. This does obey the ITrigger end-time however, 
        /// so if 'now' is after the end-time the ITrigger will not fire again.
        /// </summary>
        RescheduleNowWithExistingRepeatCount,
        /// <summary>
        /// Instructs the Scheduler that upon a mis-fire situation, 
        /// the SimpleTrigger wants to be re-scheduled to 'now' (even if the associated ICalendar excludes 'now') with the repeat count set to what it would be, 
        /// if it had not missed any firings. This does obey the ITrigger end-time however, so if 'now' is after the end-time the ITrigger will not fire again.
        /// </summary>
        RescheduleNowWithRemainingRepeatCount 
    }

    public class SimpleTrigger : BaseTrigger
    {
        public int RepeatCount { get; set; }
        public TimeSpan RepeatInterval { get; set; }
        public MisfireInstructionSimple MisfireInstruction { get; set; }
    }
}
