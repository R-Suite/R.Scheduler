using System;

namespace R.Scheduler.AssemblyPlugin.Contracts.DataContracts
{
    public class PluginSimpleTrigger
    {
        public string PluginName { get; set; }

        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }

        public DateTime StartDateTime { get; set; }
        public int RepeatCount { get; set; }
        public TimeSpan RepeatInterval { get; set; }
    }
}
