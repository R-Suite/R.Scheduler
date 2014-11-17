using System.Collections.Generic;
using R.Scheduler.Contracts.DataContracts;

namespace R.Scheduler.PipesAndFilters.Contracts
{
    public class PafTransformationDetails
    {
        public string Name { get; set; }
        public string JobDefinitionPath { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
