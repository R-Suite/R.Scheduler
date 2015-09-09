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
            UseProperties = "false";

            EnableWebApiSelfHost = true;
            WebApiBaseAddress = "http://localhost:5000/";

            EnableAuditHistory = true;
            AutoStart = true;
        }

        public PersistanceStoreType PersistanceStoreType { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public bool AutoStart { get; set; }

        /// <summary>
        /// Instruct AdoJobStore that all values in JobDataMaps will be Strings,
        /// and therefore can be stored as name-value pairs, rather than storing more complex objects 
        /// in their serialized form in the BLOB column.
        /// </summary>
        public string UseProperties { get; set; }

        public bool EnableWebApiSelfHost { get; set; }
        public string WebApiBaseAddress { get; set; }

        public bool EnableAuditHistory { get; set; }
        public string CustomFtpLibraryAssemblyName { get; set; }
    }
}
