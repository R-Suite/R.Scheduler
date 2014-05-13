using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

            string pluginAssemblyPath = Path.Combine(Environment.CurrentDirectory, "R.Scheduler.MyPlugin.dll");

            // Register Plugin
            bus.Send("R.Scheduler.Host",
                new RegisterPlugin(Guid.NewGuid())
                {
                    PluginName = "MyPlugin",
                    AssemblyPath = pluginAssemblyPath
                });

            // Execute Plugin
            bus.Send("R.Scheduler.Host",
                new SchedulePluginWithSimpleTrigger(Guid.NewGuid())
                {
                    PluginName = "MyPlugin",
                    RepeatCount = 0,
                    RepeatInterval = new TimeSpan(0, 0, 0, 5),
                });
        }
    }
}
