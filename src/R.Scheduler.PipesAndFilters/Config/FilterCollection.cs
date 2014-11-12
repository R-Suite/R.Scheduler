using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Config
{
    public sealed class FilterCollection : ConfigurationElementCollection
    {
        internal FilterElement this[int index]
        {
            get { return BaseGet(index) as FilterElement; }
            set
            {
                try
                {
                    if (BaseGet(index) != null)
                        BaseRemoveAt(index);
                }
                catch (ConfigurationErrorsException)
                {

                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        internal void Add(FilterElement extract)
        {
            base.BaseAdd(extract);
        }
    }
}
