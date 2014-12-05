using System;
using System.Collections.Specialized;
using Microsoft.Owin.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;
using IConfiguration = R.Scheduler.Interfaces.IConfiguration;

namespace R.Scheduler
{
    public sealed class Scheduler
    {
        private static IScheduler _instance;
        private static readonly object SyncRoot = new Object();

        public static IConfiguration Configuration { get; set; }

        static Scheduler()
        {
            ObjectFactory.Initialize(x => x.Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.AssembliesFromApplicationBaseDirectory();
                scan.LookForRegistries();
            }));
        }

        /// <summary>
        /// Instantiates Scheduler, including any configuration.
        /// </summary>
        /// <param name="action">A lambda that configures that sets the Scheduler configuration.</param>
        public static void Initialize(Action<IConfiguration> action = null)
        {
            if (null != _instance)
            {
                throw new Exception("Scheduler cannot be initialized after the Scheduler Instance has been created.");
            }

            var configuration = new Configuration();

            if (null != action)
            {
                action(configuration);
            }

            Configuration = configuration;

            // Initialise JobTypes modules
            var jobTypeStartups = ObjectFactory.GetAllInstances<IJobTypeStartup>();
            foreach (var jobTypeStartup in jobTypeStartups)
            {
                jobTypeStartup.Initialise(Configuration);
            }
        }

        /// <summary>
        /// Gets instance of Quartz Scheduler.
        /// Hosts WebApi endpoints and register AuditListeners.
        /// </summary>
        /// <returns></returns>
        public static IScheduler Instance()
        {
            if (null == _instance)
            {
                lock (SyncRoot)
                {
                    if (null == _instance)
                    {
                        if (null == Configuration)
                        {
                            Configuration = new Configuration();
                        }

                        ISchedulerFactory schedFact = new StdSchedulerFactory(GetProperties());
                        _instance = schedFact.GetScheduler();

                        if (Configuration.EnableAuditHistory)
                        {
                            _instance.ListenerManager.AddJobListener(new AuditJobListener(), GroupMatcher<JobKey>.AnyGroup());
                            _instance.ListenerManager.AddTriggerListener(new AuditTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
                        }

                        if (Configuration.EnableWebApiSelfHost)
                        {
                            IDisposable webApiHost = WebApp.Start<Startup>(url: Configuration.WebApiBaseAddress);
                        }
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// Halts firing and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (null != _instance)
            {
                _instance.Shutdown();
            }
        }

        private static NameValueCollection GetProperties()
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = Configuration.InstanceName;
            properties["quartz.scheduler.instanceId"] = Configuration.InstanceId;

            switch (Configuration.PersistanceStoreType)
            {
                case PersistanceStoreType.InMemory:

                    properties["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz";
                    break;

                case PersistanceStoreType.Postgre:

                    properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                    properties["quartz.jobStore.useProperties"] = Configuration.UseProperties;
                    properties["quartz.jobStore.dataSource"] = "default";
                    properties["quartz.jobStore.tablePrefix"] = Configuration.TablePrefix;
                    properties["quartz.dataSource.default.connectionString"] = Configuration.ConnectionString;
                    properties["quartz.dataSource.default.provider"] = "Npgsql-20";
                    break;

                case PersistanceStoreType.SqlServer:

                    properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                    properties["quartz.jobStore.useProperties"] = Configuration.UseProperties;
                    properties["quartz.jobStore.dataSource"] = "default";
                    properties["quartz.jobStore.tablePrefix"] = Configuration.TablePrefix;
                    properties["quartz.dataSource.default.connectionString"] = Configuration.ConnectionString;
                    properties["quartz.dataSource.default.provider"] = "SqlServer-20";
                    break;

                default:
                    throw new Exception(string.Format("Unsupported PersistanceStoreType {0}", Configuration.PersistanceStoreType));
            }

            return properties;
        }
    }
}

