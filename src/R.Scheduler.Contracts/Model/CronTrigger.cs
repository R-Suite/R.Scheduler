namespace R.Scheduler.Contracts.Model
{
    public enum MisfireInstructionCron
    {
        /// <summary>
        /// <see cref="FireOnceNow"/>
        /// </summary>
        SmartPolicy,
        /// <summary>
        /// Upon a mis-fire situation, the CronTrigger will fire now.
        /// </summary>
        FireOnceNow,
        /// <summary>
        /// Upon a mis-fire situation, 
        /// the CronTrigger will have it's next-fire-time updated to the next time in the schedule after the current time 
        /// (taking into account any associated Calendar, but it wont't fire now.
        /// </summary>
        DoNothing,
        /// <summary>
        /// If a trigger has missed several of its scheduled firings, 
        /// then several rapid firings may occur as the trigger attempt to catch back up to where it would have been. 
        /// For example, a SimpleTrigger that fires every 15 seconds which has misfired for 5 minutes 
        /// will fire 20 times once it gets the chance to fire.
        /// </summary>
        Ignore
    }

    public class CronTrigger : BaseTrigger
    {
        public string CronExpression { get; set; }
        public MisfireInstructionCron MisfireInstruction { get; set; }
    }
}
