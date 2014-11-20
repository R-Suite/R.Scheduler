using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Quartz;
using R.Scheduler.PipesAndFilters.Config;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.PipesAndFilters
{
    public class PafTransformationJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPipeLine<string> _pipeLine;
        private readonly IFiltersAssemblyLoader _assemblyLoader;
        private readonly IJobConfigurationManager _jobConfigurationManager;

        public PafTransformationJob()
        {
            _jobConfigurationManager = new JobConfigurationManager();
            _pipeLine = new PipeLine<string>();
            _assemblyLoader = new FiltersAssemblyLoader();
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string jobDefinitionPath = dataMap.GetString("jobDefinitionPath");

            Logger.Info("Entering R.Scheduler.PipesAndFilters.PafTransformationJob.Execute(). jobDefinitionPath=" + jobDefinitionPath);

            if (string.IsNullOrEmpty(jobDefinitionPath) || !File.Exists(jobDefinitionPath))
            {
                Logger.Error(string.Format("plugin file '{0}' does not exist.", jobDefinitionPath));
                return;
            }

            FilterCollection filters = GetFilters(jobDefinitionPath);

            AddFiltersToPipeline(filters, jobDefinitionPath);

            var sList = GetStartList(jobDefinitionPath);
            if (sList != null)
            {
                _pipeLine.SetStartPipeLine(sList);
            }

            _pipeLine.Execute(jobDefinitionPath);
        }

        private FilterCollection GetFilters(string jobDefinitionPath)
        {
            return _jobConfigurationManager.GetFilters<PipesAndFiltersConfigurationSection>(jobDefinitionPath).Filters;
        }

        private IEnumerable<string> GetStartList(string jobName)
        {
            IList<string> startList = new List<string>();
            if (_jobConfigurationManager.GetFilters<PipesAndFiltersConfigurationSection>(jobName).StartParam != null)
            {
                foreach (StartParamElement item in _jobConfigurationManager.GetFilters<PipesAndFiltersConfigurationSection>(jobName).StartParam)
                {
                    startList.Add(item.KeyName);
                }
            }
            return startList;
        }

        private void AddFiltersToPipeline(FilterCollection filters, string jobDefinitionPath)
        {
            Logger.Info("Registering Filters");

            foreach (FilterElement f in filters)
            {
                var filter = CreateFilterAssembly(f, jobDefinitionPath);
                _pipeLine.Register(filter);
            }
        }

        private IFilter<string> CreateFilterAssembly(FilterElement f, string jobDefinitionPath)
        {
            return _assemblyLoader.LoadFilterFromPath<string>(f.AssemblyPath, jobDefinitionPath);
        }
    }
}
