using System.Collections.Generic;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Contracts.JobTypes.PipesAndFilters.Model
{
    public class PafTransformationDetails
    {
        public string Name { get; set; }
        public string JobDefinitionPath { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
