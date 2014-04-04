using System.Reflection;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.Core
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
