using System;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.MyPlugin
{
    public class Plugin : BaseJobPlugin
    {
        public override void Execute()
        {
            Console.WriteLine("Executing MyPlugin");
        }
    }
}
