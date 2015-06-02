using System;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin;

namespace R.Scheduler.FakeJobPluginWithError
{
    public class Plugin : BaseJobPlugin
    {
        public override string Name
        {
            get { return "TestErrorPlugin"; }
        }

        public override void Execute()
        {
            throw new Exception("test exception");
        }
    }
}
