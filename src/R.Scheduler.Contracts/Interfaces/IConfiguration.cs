using R.MessageBus.Interfaces;

namespace R.Scheduler.Contracts.Interfaces
{
    public interface IConfiguration
    {
        PersistanceStoreType PersistanceStoreType { get; set; }
        string ConnectionString { get; set; }
        string TablePrefix { get; set; }
        string InstanceName { get; set; }
        string InstanceId { get; set; }
        string UseProperties { get; set; }

        ITransportSettings TransportSettings { get; set; }
    }
}
