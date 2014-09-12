using System.IO;
using System.Reflection;
using log4net;
using R.Scheduler.Interfaces;

namespace R.Scheduler.JobTypes.Plugin
{
    public class PluginJob : IJobTypeManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPluginStore _pluginRepository;

        public PluginJob(IPluginStore pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public void Register(string name, params string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Logger.ErrorFormat("Error registering plugin {0}. Invalid assembly path {1}", name, args[0]);
                return;
            }
            //todo: verify valid plugin.. reflection?

            _pluginRepository.RegisterPlugin(new Contracts.DataContracts.Plugin()
            {
                AssemblyPath = args[0],
                Name = name,
                Status = "registered"
            });
        }

        //public void RemovePlugin(string pluginName)
        //{
        //    var jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(pluginName));

        //    _scheduler.DeleteJobs(jobKeys.ToList());

        //    int result = _pluginStore.RemovePlugin(pluginName);

        //    if (result == 0)
        //    {
        //        Logger.WarnFormat("Error removing from data store. Plugin {0} not found", pluginName);
        //    }
        //}

    }
}
