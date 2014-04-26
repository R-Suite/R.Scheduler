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

            bus.Send("R.Scheduler", new RegisterPlugin(Guid.NewGuid()){ PluginName = "FakePlugin", AssemblyPath = @"C:\GIT\RSuite\R.Scheduler\src\R.Scheduler.IntegrationTests\Resourses\R.Scheduler.FakeJobPlugin.dll"});


            bus.Send("R.Scheduler",
                new SchedulePluginWithSimpleTrigger(Guid.NewGuid())
                {
                    PluginName = "FakePlugin",
                    //RepeatCount = 1,
                    //RepeatInterval = new TimeSpan(0,0,0,5),
                });
        }
    }
}
