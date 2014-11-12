using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Config
{
    public sealed class StartParamElement : ConfigurationElement
    {
        [ConfigurationProperty("keyName", IsRequired = true)]
        public string KeyName
        {
            get { return this["keyName"] as string; }
            set { this["keyName"] = value; }
        }
    }
}
