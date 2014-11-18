using System.Collections.Generic;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model
{
    public class PluginDetails
    {
        public string Name { get; set; }
        public string AssemblyPath { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
