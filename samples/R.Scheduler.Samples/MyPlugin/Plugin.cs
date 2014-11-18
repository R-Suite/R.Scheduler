using System;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin;

namespace MyPlugin
{
    public class Plugin : BaseJobPlugin
    {
        public override void Execute()
        {
            Console.WriteLine("Executing MyPlugin");
        }
    }
}
