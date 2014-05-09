using System.Reflection;

namespace R.Scheduler.Contracts.Interfaces
{
<<<<<<< HEAD:src/R.Scheduler.Core/BasePlugin.cs
    //public abstract class BasePlugin : IJobPlugin
    //{
    //    public virtual string Name
    //    {
    //        get { return Assembly.GetExecutingAssembly().GetName().Name; }
    //    }
=======
    public abstract class BaseJobPlugin : IJobPlugin
    {
        public virtual string Name
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name; }
        }
>>>>>>> 35fe9ddc5ef62fe675d27c771841c94f55be3325:src/R.Scheduler.Contracts/Interfaces/BaseJobPlugin.cs

    //    public abstract void Execute();
    //}
}
