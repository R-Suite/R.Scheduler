using System.Reflection;

namespace R.Scheduler.Contracts.Interfaces
{
    public abstract class BasePlugin : IJobPlugin
    {
        public virtual string Name
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name; }
        }

        public abstract void Execute();
    }
}
