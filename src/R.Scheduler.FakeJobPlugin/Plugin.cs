using R.Scheduler.Contracts.Interfaces;

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
        }
    }
}
