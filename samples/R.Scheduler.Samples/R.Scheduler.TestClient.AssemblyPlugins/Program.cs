using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.TestClient.AssemblyPlugins
{
    class Program
    {
        private static readonly string PluginAssemblyPath = Path.Combine(Environment.CurrentDirectory, "MyPlugin.dll");
        private const string Url = @"http://localhost:5000/";
        private static Guid _jobId;

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("<1> Register MyPlugin (AssemblyPluginJob)");
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
            var job = new PluginJob
            {
                AssemblyPath = PluginAssemblyPath,
                JobName = "MyAssemblyPluginJob",
                JobGroup = "MyJobGroup"
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/plugins/", job).Result;
                string resultStr = response.Content.ReadAsStringAsync().Result;

                dynamic result = JsonConvert.DeserializeObject<dynamic>(resultStr);

                if (result != null)
                {
                    Console.WriteLine(resultStr);
                    _jobId = new Guid(result.id.ToString());
                }
            }
        }

        /// <summary>
        /// Execute plugin immediately
        /// </summary>
        private static void ExecutePlugin()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsync("api/jobs/" + _jobId, null).Result;
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
            var trigger = new SimpleTrigger
            {
                Name = "MyPluginTestTrigger",
                Group = "DEFAULT",
                JobName = "MyAssemblyPluginJob",
                JobGroup = "MyJobGroup",
                StartDateTime = DateTime.Now.AddSeconds(3),
                RepeatCount = 3,
                RepeatInterval = new TimeSpan(0,0,0,3)
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/simpleTriggers", trigger).Result;
                string result = response.Content.ReadAsStringAsync().Result;

                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}
