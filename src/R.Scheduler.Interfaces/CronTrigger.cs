namespace R.Scheduler.Interfaces
{
    public class CronTrigger : BaseTrigger
    {
        public string CronExpression { get; set; }
    }
}
