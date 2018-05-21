using System.Collections.Generic;
using Quartz;
using R.Scheduler.Ftp;
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
            ThreadCount = 10;

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
        public int ThreadCount { get; set; }

        /// <summary>
        /// Instruct AdoJobStore that all values in JobDataMaps will be Strings,
        /// and therefore can be stored as name-value pairs, rather than storing more complex objects 
        /// in their serialized form in the BLOB column.
        /// </summary>
        public string UseProperties { get; set; }

        public bool EnableWebApiSelfHost { get; set; }

        public string WebApiBaseAddress { get; set; }

        public bool EnableAuditHistory { get; set; }

        /// <summary>
        /// Assembly name that contains custom implementation of <see cref="IFtpLibrary"/>
        /// </summary>
        public string CustomFtpLibraryAssemblyName { get; set; }

        /// <summary>
        /// Assembly name that contains custom implementation of <see cref="ITriggerListener"/>
        /// </summary>
        public IList<string> CustomTriggerListenerAssemblyNames { get; set; }

        /// <summary>
        /// Assembly name that contains custom implementation of <see cref="ISchedulerListener"/>
        /// </summary>
        public IList<string> CustomSchedulerListenerAssemblyNames { get; set; }

        /// <summary>
        /// Assembly name that contains custom implementation of <see cref="IJobListener"/>
        /// </summary>
        public IList<string> CustomJobListenerAssemblyNames { get; set; }

        /// <summary>
        /// Assembly name that contains custom implementation of <see cref="IAuthorize"/>
        /// </summary>
        public string CustomAuthorizationAssemblyName { get; set; }

        /// <summary>
        /// Assembly name that contains custom WebApp settings
        /// </summary>
        public string CustomWebAppSettingsAssemblyName { get; set; }
    }
}
