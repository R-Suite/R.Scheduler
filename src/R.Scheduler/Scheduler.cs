using System;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using R.MessageBus;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;
using IConfiguration = R.Scheduler.Contracts.Interfaces.IConfiguration;

namespace R.Scheduler
{
    public sealed class Scheduler
    {
        private static IScheduler _instance;
        private static readonly object SyncRoot = new Object();

        public static IConfiguration Configuration { get; set; }

        static Scheduler()
        {
            ObjectFactory.Initialize(x=>x.AddRegistry<SmRegistry>());
        }

        /// <summary>
        /// Instantiates Scheduler, including any configuration.
        /// </summary>
        /// <param name="action">A lambda that configures that sets the Scheduler configuration.</param>
        public static void Initialize(Action<IConfiguration> action)
        {
            var configuration = new Configuration();
            action(configuration);

            Configuration = configuration;
        }

        /// <summary>
        /// Gets instance of Quartz Scheduler
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

                        // Initialise message bus
                        IBus bus = Bus.Initialize(config =>
                        {
                            config.ScanForMesssageHandlers = true;
                        });

                        bus.StartConsuming();
                    }
                }
            }

            return _instance;
        }

        private static NameValueCollection GetProperties()
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = Configuration.InstanceName;
            properties["quartz.scheduler.instanceId"] = Configuration.InstanceId;

            switch (Configuration.PersistanceStoreType)
            {
                case PersistanceStoreType.InMemory:
                    // Set implementation of IPluginStore in IoC Container. 
                    ObjectFactory.Configure(x => x.For<IPluginStore>().Use<InMemoryPluginStore>());
                    
                    // Set properties
                    properties["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz";
                    break;
                case PersistanceStoreType.Postgre:
                    //Sets implementation of IPluginStore in IoC Container. 
                    ObjectFactory.Configure(x =>x.For<IPluginStore>().Use<PostgrePluginStore>().Ctor<string>("connectionString").Is(Configuration.ConnectionString));
                    
                    // Set properties
                    properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                    properties["quartz.jobStore.useProperties"] = Configuration.UseProperties;
                    properties["quartz.jobStore.dataSource"] = "default";
                    properties["quartz.jobStore.tablePrefix"] = Configuration.TablePrefix;
                    properties["quartz.dataSource.default.connectionString"] = Configuration.ConnectionString;
                    properties["quartz.dataSource.default.provider"] = "Npgsql-20";
                    break;
                default:
                    throw new Exception(string.Format("Unsupported PersistanceStoreType {0}", Configuration.PersistanceStoreType));
            }

            return properties;
        }
    }
}
