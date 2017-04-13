namespace R.Scheduler.Contracts.JobTypes.AssemblyPlugin
{
    public interface IJobPlugin
    {
        string Name { get; } 
        void Execute();
    }
}
