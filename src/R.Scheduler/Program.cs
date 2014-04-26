using Quartz;
using R.MessageBus;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialise scheduler
            Scheduler.SetPluginStore(PluginStoreType.InMemory);
            IScheduler sched = Scheduler.Instance();
            sched.Start();

            // Initialise message bus
            IBus bus = Bus.Initialize(config =>
            {
                config.ScanForMesssageHandlers = true;
            });

            bus.StartConsuming();
        }
    }
}
