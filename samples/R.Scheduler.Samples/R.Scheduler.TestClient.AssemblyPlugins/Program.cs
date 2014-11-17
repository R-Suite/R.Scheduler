using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace R.Scheduler.TestClient.AssemblyPlugins
{
    class Program
    {
        private static readonly string PluginAssemblyPath = Path.Combine(Environment.CurrentDirectory, "MyPlugin.dll");
        private const string Url = @"http://localhost:5000/";

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("<1> Register MyPlugin (plugins must be registered first)");
            Console.WriteLine("<2> Execute MyPlugin (immediately)");
            Console.WriteLine("<3> Schedule MyPlugin with SimpleTrigger (executes 3 times in 3-second intervals)");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("<0> Exit");

            int choice = -1;
            while (choice != 0)
            {
                choice = Int32.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        RegisterPlugin();
                        break;
                    case 2:
                        ExecutePlugin();
                        break;
                    case 3:
                        SchedulePluginWithSimpleTrigger();
                        break;
                }
            }
        }

        /// <summary>
        /// Register plugin. All plugin must be registered first.
        /// </summary>
        private static void RegisterPlugin()
        {
            var dataDict = new Dictionary<string, string>
            {
                {"Name", "MyPlugin"},
                {"AssemblyPath", PluginAssemblyPath}
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new FormUrlEncodedContent(dataDict);
                var response = client.PostAsync("api/plugins/", requestContent).Result;
                string result = response.Content.ReadAsStringAsync().Result;

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        /// <summary>
        /// Execute plugin immediately
        /// </summary>
        private static void ExecutePlugin()
        {
            var dataDict = new Dictionary<string, string>{{"", "MyPlugin"}};

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new FormUrlEncodedContent(dataDict);
                var response = client.PostAsync("api/plugins/execute/", requestContent).Result;
                string result = response.Content.ReadAsStringAsync().Result;

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }

        /// <summary>
        /// Schedule plugin to execute 3 seconds in the future and then 3 more times with 3 second intervals
        /// </summary>
        private static void SchedulePluginWithSimpleTrigger()
        {
            var dataDict = new Dictionary<string, string>
            {
                {"TriggerName", "MyPluginTestTrigger"},
                {"StartDateTime", DateTime.Now.AddSeconds(3).ToString("O") },
                {"RepeatCount", "3"},
                {"RepeatInterval", "00:00:03"}
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new FormUrlEncodedContent(dataDict);
                var response = client.PostAsync("api/plugins/MyPlugin/simpleTriggers/", requestContent).Result;
                string result = response.Content.ReadAsStringAsync().Result;

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
