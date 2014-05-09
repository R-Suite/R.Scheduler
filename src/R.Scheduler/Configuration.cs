using R.MessageBus;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts;
using IConfiguration = R.Scheduler.Contracts.Interfaces.IConfiguration;

namespace R.Scheduler
{
    public class Configuration : IConfiguration
    {
        public Configuration()
        {
            PersistanceStoreType = PersistanceStoreType.InMemory;
            TablePrefix = "QRTZ_";
            InstanceName = "RScheduler";
            InstanceId = "instance_one";
            UseProperties = "true";
            
            var messageBusConfig = new MessageBus.Configuration();
            TransportSettings = messageBusConfig.TransportSettings;
        }

        public PersistanceStoreType PersistanceStoreType { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public string UseProperties { get; set; }

        public ITransportSettings TransportSettings { get; set; }
    }
}
