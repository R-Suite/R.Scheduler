using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.FakeJobPlugin
{
    public class Plugin : BasePlugin
    {
        public override string Name
        {
            get { return "TestPlugin"; }
        }

        public override void Execute()
        {
        }
    }
}
