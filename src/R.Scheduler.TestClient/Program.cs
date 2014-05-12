using System;
using R.MessageBus;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IBus bus = Bus.Initialize();

            bus.Send("R.Scheduler.Host", new RegisterPlugin(Guid.NewGuid()){ PluginName = "FakePlugin", AssemblyPath = @"C:\GitHub\R.Scheduler\src\R.Scheduler.IntegrationTests\Resourses\R.Scheduler.FakeJobPlugin.dll"});


            bus.Send("R.Scheduler.Host",
                new SchedulePluginWithSimpleTrigger(Guid.NewGuid())
                {
                    PluginName = "FakePlugin",
                    RepeatCount = 0,
                    RepeatInterval = new TimeSpan(0, 0, 0, 5),
                });
        }
    }
}
