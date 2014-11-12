using System.Collections.Generic;

namespace R.Scheduler.PipesAndFilters.Interfaces
{
    public interface IPipeLine<T>
    {
        void Execute(string jobName);
        void Register(IFilter<T> filter);
        void SetStartPipeLine(IEnumerable<T> pipeLineCurrentList);
    }
}
