using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Interfaces
{
    public interface IJobConfigurationManager
    {
        T GetFilters<T>(string jobDefinitionPath) where T : ConfigurationSection;
    }
}
