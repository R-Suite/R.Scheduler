using System;

namespace R.Scheduler.AssemblyPlugin.Contracts.DataContracts
{
    public class Plugin
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AssemblyPath { get; set; }
        public string Status { get; set; }
    }
}
