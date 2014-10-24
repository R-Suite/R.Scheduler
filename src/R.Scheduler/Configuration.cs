using R.Scheduler.Interfaces;
using IConfiguration = R.Scheduler.Interfaces.IConfiguration;

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

            EnableWebApiSelfHost = true;
            WebApiBaseAddress = "http://localhost:5000/";
        }

        public PersistanceStoreType PersistanceStoreType { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public string UseProperties { get; set; }

        public bool EnableWebApiSelfHost { get; set; }
        public string WebApiBaseAddress { get; set; }
    }
}
