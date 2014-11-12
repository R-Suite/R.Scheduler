using System.Configuration;

namespace R.Scheduler.PipesAndFilters.Config
{
    public sealed class StartParamCollection : ConfigurationElementCollection
    {
        public StartParamElement this[int index]
        {
            get { return BaseGet(index) as StartParamElement; }
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
            return new StartParamElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        public void Add(StartParamElement extract)
        {
            base.BaseAdd(extract);
        }
    }
}
