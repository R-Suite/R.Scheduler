using System;

namespace R.Scheduler.Contracts.JobTypes.PipesAndFilters.Model
{
    public class PafTransformation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string JobDefinitionPath { get; set; }
    }
}
