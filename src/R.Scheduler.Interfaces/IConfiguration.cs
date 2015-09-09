namespace R.Scheduler.Interfaces
{
    public interface IConfiguration
    {
        PersistanceStoreType PersistanceStoreType { get; set; }
        string ConnectionString { get; set; }
        string TablePrefix { get; set; }
        string InstanceName { get; set; }
        string InstanceId { get; set; }
        bool AutoStart { get; set; }

        /// <summary>
        /// Instruct AdoJobStore that all values in JobDataMaps will be Strings,
        /// and therefore can be stored as name-value pairs, rather than storing more complex objects 
        /// in their serialized form in the BLOB column.
        /// </summary>
        string UseProperties { get; set; }

        bool EnableWebApiSelfHost { get; set; }
        string WebApiBaseAddress { get; set; }

        bool EnableAuditHistory { get; set; }

        string CustomFtpLibraryAssemblyName { get; set; }
    }
}
