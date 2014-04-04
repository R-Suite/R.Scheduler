namespace R.Scheduler.Contracts.Interfaces
{
    public class RegisteredPlugin
    {
        public string Name { get; set; }
        public string AssemblyPath { get; set; }
        public string Status { get; set; }
    }
}
