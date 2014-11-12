namespace R.Scheduler.PipesAndFilters.Interfaces
{
    public interface IFiltersAssemblyLoader
    {
        IFilter<T> LoadFilterFromPath<T>(string filterAssembleyPath, string jobDefinitionPath);
    }
}
