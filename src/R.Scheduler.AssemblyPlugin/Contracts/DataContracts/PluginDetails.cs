using System.Collections.Generic;
using R.Scheduler.Contracts.DataContracts;

namespace R.Scheduler.AssemblyPlugin.Contracts.DataContracts
{
    public class PluginDetails
    {
        public string PluginName { get; set; }
        public string AssemblyPath { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
