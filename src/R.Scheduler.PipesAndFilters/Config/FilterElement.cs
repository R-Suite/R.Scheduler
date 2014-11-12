using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Config
{
    internal sealed class FilterElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        internal string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("assemblyPath", IsRequired = true)]
        internal string AssemblyPath
        {
            get { return this["assemblyPath"] as string; }
            set { this["assemblyPath"] = value; }
        }
    }
}
