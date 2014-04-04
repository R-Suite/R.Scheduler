namespace R.Scheduler.Contracts.Interfaces
{
    public interface IJobPlugin
    {
        string Name { get; } 
        void Execute();
    }
}
