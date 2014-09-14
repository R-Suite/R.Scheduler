namespace R.Scheduler.AssemblyPlugin.Contracts.Interfaces
{
    public interface IJobPlugin
    {
        string Name { get; } 
        void Execute();
    }
}
