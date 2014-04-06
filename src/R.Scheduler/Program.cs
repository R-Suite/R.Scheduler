using Quartz;

namespace R.Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // get a scheduler
            IScheduler sched = Scheduler.Instance();
            sched.Start();
        }
    }
}
