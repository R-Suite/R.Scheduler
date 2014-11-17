using System;
using R.Scheduler.AssemblyPlugin.Contracts.Interfaces;

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
