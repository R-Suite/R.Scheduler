namespace R.Scheduler.Contracts.Interfaces
{
    public interface IConfiguration
    {
        /// <summary>
        /// Sets the plugin store.
        /// </summary>
        /// <typeparam name="T">The type must be a class that implements IPluginStore.</typeparam>
        void SetPluginStore<T>() where T : class, IPluginStore;
    }
}
