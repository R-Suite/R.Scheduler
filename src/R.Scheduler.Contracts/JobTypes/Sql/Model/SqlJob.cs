namespace R.Scheduler.Contracts.JobTypes.Sql.Model
{
    public class SqlJob : BaseJob
    {
        public string ProviderAssemblyName { get; set; }
        public string ConnectionClass { get; set; }
        public string CommandClass { get; set; }
        public string DataAdapterClass { get; set; }
        public string CommandStyle { get; set; }
        public string ConnectionString { get; set; }
        public string NonQueryCommand { get; set; }

    }
}
