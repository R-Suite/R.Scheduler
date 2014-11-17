using System;
using System.IO;
using System.Reflection;
using log4net;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;

namespace R.Scheduler.PipesAndFilters
{
    public class PafTransformationManager : IJobTypeManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly ICustomJobStore _repository;
        private readonly ISchedulerCore _schedulerCore;

        public PafTransformationManager(ICustomJobStore repository, ISchedulerCore schedulerCore)
        {
            _repository = repository;
            _schedulerCore = schedulerCore;
        }

        /// <summary>
        /// Registers new PafTransformation. No jobs are scheduled at this point.
        /// </summary>
        /// <param name="name">plugin name</param>
        /// <param name="args">assembly file path</param>
        public void Register(string name, params string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Logger.ErrorFormat("Error registering PafTransformation {0}. Invalid job definition path {1}", name, args[0]);
                throw new FileNotFoundException(string.Format("Invalid job definition path {0}", args[0]));
            }
            //todo: verify valid job definition.. xml validation?

            _repository.RegisterJob(new CustomJob
            {
                Params = args[0],
                Name = name,
                JobType = "PaF"
            });
        }

        /// <summary>
        /// Removes PafTransformation and deletes all the related triggers/jobs.
        /// </summary>
        public void Remove(Guid id)
        {
            var registeredPlugin = _repository.GetRegisteredJob(id);

            // plugin name is a job group
            _schedulerCore.RemoveJobGroup(registeredPlugin.Name);

            _repository.Remove(id);
        }
    }
}
