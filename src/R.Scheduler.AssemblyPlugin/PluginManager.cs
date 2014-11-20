using System;
using System.IO;
using System.Reflection;
using log4net;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;

namespace R.Scheduler.AssemblyPlugin
{
    public class PluginManager : IJobTypeManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _pluginRepository;
        private readonly ISchedulerCore _schedulerCore;

        public PluginManager(ICustomJobStore pluginRepository, ISchedulerCore schedulerCore)
        {
            _pluginRepository = pluginRepository;
            _schedulerCore = schedulerCore;
        }

        /// <summary>
        /// Registers new plugin. No jobs are scheduled at this point.
        /// </summary>
        /// <param name="name">plugin name</param>
        /// <param name="args">assembly file path</param>
        public void Register(string name, params string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Logger.ErrorFormat("Error registering plugin {0}. Invalid assembly path {1}", name, args[0]);
                throw new FileNotFoundException(string.Format("Invalid assembly path {0}", args[0]));
            }
            //todo: verify valid plugin.. reflection?

            _pluginRepository.RegisterJob(new CustomJob
            {
                Params = args[0],
                Name = name,
                JobType = typeof(AssemblyPluginJob).Name
            });
        }

        /// <summary>
        /// Removes plugin and deletes all the related triggers/jobs.
        /// </summary>
        public void Remove(Guid id)
        {
            _schedulerCore.RemoveTriggersOfJobType(typeof(AssemblyPluginJob));

            _pluginRepository.Remove(id);
        }
    }
}
