using System.Collections.Generic;

namespace R.Scheduler.AssemblyPlugin.Contracts.DataContracts
{
    public class PluginDetails
    {
        public string PluginName { get; set; }
        public string AssemblyPath { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
