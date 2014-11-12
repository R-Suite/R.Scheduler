using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.PipesAndFilters
{
    public sealed class PipeLine<T> : IPipeLine<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly List<IFilter<T>> _filters = new List<IFilter<T>>();

        private IEnumerable<T> _pipeLineCurrentList = new List<T>();

        public void SetStartPipeLine(IEnumerable<T> pipeLineCurrentList)
        {
            _pipeLineCurrentList = pipeLineCurrentList;
        }

        public void Execute(string jobPath)
        {
            Logger.Info("Entering PipeLine.Execute()");

            if (_filters == null || _filters.Count == 0)
            {
                Logger.Error("PipeLine.Execute(): No Filters registered for Job " + jobPath);
                throw new Exception("At least one Filter must be registered.");
            }

            foreach (var item in _filters)
            {
                Logger.Info(string.Format("Filter {0}", item.GetType().Name));
                _pipeLineCurrentList = item.Execute(_pipeLineCurrentList);
            }
            IEnumerator<T> enumerator = _pipeLineCurrentList.GetEnumerator();
            while (enumerator.MoveNext());
        }

        public void Register(IFilter<T> filter)
        {
            _filters.Add(filter);
        }
    }
}
