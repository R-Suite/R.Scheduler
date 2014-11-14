using System;
using System.IO;
using R.Scheduler.AssemblyPlugin.Contracts.Interfaces;

namespace R.Scheduler.FakeJobPlugin
{
    public class Plugin : BaseJobPlugin
    {
        public override string Name
        {
            get { return "TestPlugin"; }
        }

        public override void Execute()
        {
            using (var writer = new StreamWriter("FakeJobPlugin.txt", append:false))
            {
                writer.Write(DateTime.UtcNow);
            }
        }
    }
}
