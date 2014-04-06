using System;
using R.Scheduler.Contracts.Interfaces;

namespace R.Scheduler.Persistance
{
    public class PostgrePluginStore : IPluginStore
    {
        public Plugin GetRegisteredPlugin(string pluginName)
        {
            throw new NotImplementedException();
        }

        public void RegisterPlugin(Plugin plugin)
        {
            throw new NotImplementedException();
        }
    }
}
