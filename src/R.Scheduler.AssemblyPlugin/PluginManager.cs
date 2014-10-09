using System;
using System.IO;
using System.Reflection;
using log4net;
using R.Scheduler.AssemblyPlugin.Contracts.DataContracts;
using R.Scheduler.AssemblyPlugin.Interfaces;
using R.Scheduler.Interfaces;

namespace R.Scheduler.AssemblyPlugin
{
    public class PluginManager : IJobTypeManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPluginStore _pluginRepository;
        private readonly ISchedulerCore _schedulerCore;

        public PluginManager(IPluginStore pluginRepository, ISchedulerCore schedulerCore)
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
                return;
            }
            //todo: verify valid plugin.. reflection?

            _pluginRepository.RegisterPlugin(new Plugin
            {
                AssemblyPath = args[0],
                Name = name,
            });
        }

        /// <summary>
        /// Removes plugin and deletes all the related triggers/jobs.
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            // plugin name is a job group
            _schedulerCore.RemoveJobGroup(name);

            int result = _pluginRepository.RemovePlugin(name);

            if (result == 0)
            {
                Logger.WarnFormat("Error removing from data store. Plugin {0} not found", name);
            }
        }
    }
}
