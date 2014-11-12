using System.Collections.Generic;

namespace R.Scheduler.PipesAndFilters.Interfaces
{
    public interface IFilter<T>
    {
        IEnumerable<T> Execute(IEnumerable<T> input);
    }
}
