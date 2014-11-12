using System;
using System.Configuration;
using System.Reflection;
using log4net;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.PipesAndFilters.Config
{
    internal sealed class JobConfigurationManager : IJobConfigurationManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Configuration _config;

        public T GetFilters<T>(string jobDefinitionPath) where T : ConfigurationSection
        {
            try
            {
                var ecfm = new ExeConfigurationFileMap(jobDefinitionPath) { ExeConfigFilename = jobDefinitionPath };
                _config = ConfigurationManager.OpenMappedExeConfiguration(ecfm, ConfigurationUserLevel.None);
            }
            catch (Exception exception)
            {
                Logger.Error(string.Format("Job {0} not found", jobDefinitionPath), exception);
            }

            return GetSection<T>("jobPipeline");
        }

        private T GetSection<T>(string sectionName) where T : ConfigurationSection
        {
            return (T)_config.GetSection(sectionName);
        }
    }
}
