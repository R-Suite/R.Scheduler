using System;

namespace R.Scheduler.PipesAndFilters.Contracts
{
    public class PafTransformation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string JobDefinitionPath { get; set; }
    }
}
