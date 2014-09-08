using R.MessageBus.Interfaces;
using R.Scheduler.Contracts;
using R.Scheduler.Interfaces;

namespace R.Scheduler
{
    public class Configuration : Interfaces.IConfiguration
    {
        public Configuration()
        {
            PersistanceStoreType = PersistanceStoreType.InMemory;
            TablePrefix = "QRTZ_";
            InstanceName = "RScheduler";
            InstanceId = "instance_one";
            UseProperties = "true";

            EnableMessageBusSelfHost = false;
            EnableWebApiSelfHost = true;
            WebApiBaseAddress = "http://localhost:5000/";
            
            var messageBusConfig = new MessageBus.Configuration();
            TransportSettings = messageBusConfig.TransportSettings;
        }

        public PersistanceStoreType PersistanceStoreType { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public string UseProperties { get; set; }

        public bool EnableMessageBusSelfHost { get; set; }
        public bool EnableWebApiSelfHost { get; set; }
        public string WebApiBaseAddress { get; set; }

        public ITransportSettings TransportSettings { get; set; }
    }
}
