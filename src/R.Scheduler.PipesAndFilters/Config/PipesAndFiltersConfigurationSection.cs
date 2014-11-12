using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Config
{
    public class PipesAndFiltersConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// [Required] Filters
        /// </summary>
        [ConfigurationProperty("filters")]
        [ConfigurationCollection(typeof(FilterCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public virtual FilterCollection Filters
        {
            get { return this["filters"] as FilterCollection; }
            set { }
        }

        /// <summary>
        /// [Required] The startList
        /// </summary>
        [ConfigurationProperty("startParams")]
        [ConfigurationCollection(typeof(StartParamCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public virtual StartParamCollection StartParam
        {
            get { return this["startParams"] as StartParamCollection; }
            set { }
        }
    }
}
