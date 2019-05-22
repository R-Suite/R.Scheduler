using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.Owin.Hosting;
using Owin;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using R.Scheduler.Core;
using R.Scheduler.Persistence;
using R.Scheduler.Interfaces;
using StructureMap;
using IConfiguration = R.Scheduler.Interfaces.IConfiguration;
using log4net;

namespace R.Scheduler
{
    public sealed class Scheduler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IScheduler _instance;
        private static IPersistenceStore _persistenceStore;
        private static readonly object SyncRoot = new Object();

        public static IConfiguration Configuration { get; set; }

        static Scheduler()
        {
            SchedulerContainer.Container = new Container(x => x.Scan(scan =>
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Initialize(Action<IConfiguration> action = null)
        {
            if (null != _instance)
            {
                throw new InvalidOperationException("Scheduler cannot be initialized after the Scheduler Instance has been created.");
            }

            var configuration = new Configuration();
            if (null != action)
            {
                action(configuration);
            }

            Configuration = configuration;

            // Ensure container resolves persistence store based on configuration settings


            // Create Temp store,
            _persistenceStore = new InMemoryStore();
            // Determine store type from configuration
            switch (Configuration.PersistenceStoreType)
            {
                case PersistenceStoreType.Postgre:
                    _persistenceStore = new PostgreStore(Configuration.ConnectionString);
                    break;
                case PersistenceStoreType.SqlServer:
                    _persistenceStore = new SqlServerStore(Configuration.ConnectionString);
                    break;
                case PersistenceStoreType.InMemory:
                    _persistenceStore = new InMemoryStore();
                    break;
            }

            // Inject the persistence store after initialization
            SchedulerContainer.Container.Inject(_persistenceStore);

            // Initialise JobTypes modules
            var jobTypeStartups = SchedulerContainer.Container.GetAllInstances<IJobTypeStartup>();
            foreach (var jobTypeStartup in jobTypeStartups)
            {
                jobTypeStartup.Initialise(Configuration);
            }


            if (configuration.AutoStart)
            {
                IScheduler sched = Instance();
                sched.Start();
                SchedulerContainer.Container.Inject(sched);
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
                            _instance.ListenerManager.AddJobListener(new AuditJobListener(_persistenceStore), GroupMatcher<JobKey>.AnyGroup());
                            _instance.ListenerManager.AddTriggerListener(new AuditTriggerListener(_persistenceStore), GroupMatcher<TriggerKey>.AnyGroup());
                        }

                        // Custom listeners
                        AddCustomTriggerListeners();
                        AddCustomJobListeners();
                        AddCustomSchedulerListeners();

                        // Custom Authorization
                        AddCustomAuthorization();
                        AddCustomPermissionsManager();

                        if (Configuration.EnableWebApiSelfHost)
                        {
                            // If custom WebApp Settings is defined in a custom assembly - load the assembly, get the relevant type and method,
                            // make  generic method and invoke it.
                            if (!string.IsNullOrEmpty(Configuration.CustomWebAppSettingsAssemblyName))
                            {
                                var asm = GetAssembly(Configuration.CustomWebAppSettingsAssemblyName);

                                Type startupType = null;
                                var allTypes = asm.GetTypes();
                                foreach (var type in allTypes)
                                {
                                    var allMethods = type.GetMethods();

                                    foreach (MethodInfo methodInfo in allMethods)
                                    {
                                        ParameterInfo pi = methodInfo.GetParameters().FirstOrDefault(q => q.ParameterType == typeof(IAppBuilder));

                                        if (null != pi)
                                        {
                                            startupType = type;
                                            break;
                                        }
                                    }

                                    if (startupType != null)
                                    {
                                        break;
                                    }
                                }

                                MethodInfo miWebAppStart = typeof (Scheduler).GetMethod("WebAppStart", BindingFlags.NonPublic | BindingFlags.Static);
                                MethodInfo genericMiWebAppStart = miWebAppStart.MakeGenericMethod(startupType);
                                genericMiWebAppStart.Invoke(null, new object[] {Configuration.WebApiBaseAddress});
                            }
                            else
                            {
                                // No custom WebApp Settings defined, use the built-in default.
                                WebApp.Start<Startup>(url: Configuration.WebApiBaseAddress);
                            }
                        }
                    }
                }
            }

            return _instance;
        }

        private static void WebAppStart<T>(string webApiBaseAddress)
        {
            WebApp.Start<T>(url: webApiBaseAddress);
        }

        /// <summary>
        /// Halts firing and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (null != _instance)
            {
                _instance.Shutdown();
                _instance = null;
            }
        }

        private static NameValueCollection GetProperties()
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = Configuration.InstanceName;
            properties["quartz.scheduler.instanceId"] = Configuration.InstanceId;
            properties["quartz.threadPool.threadCount"] = Configuration.ThreadCount.ToString();

            switch (Configuration.PersistenceStoreType)
            {
                case PersistenceStoreType.InMemory:

                    properties["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz";
                    break;

                case PersistenceStoreType.Postgre:

                    properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                    properties["quartz.jobStore.useProperties"] = Configuration.UseProperties;
                    properties["quartz.jobStore.dataSource"] = "default";
                    properties["quartz.jobStore.tablePrefix"] = Configuration.TablePrefix;
                    properties["quartz.dataSource.default.connectionString"] = Configuration.ConnectionString;
                    properties["quartz.dataSource.default.provider"] = "Npgsql-20";
                    break;

                case PersistenceStoreType.SqlServer:

                    properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
                    properties["quartz.jobStore.useProperties"] = Configuration.UseProperties;
                    properties["quartz.jobStore.dataSource"] = "default";
                    properties["quartz.jobStore.tablePrefix"] = Configuration.TablePrefix;
                    properties["quartz.dataSource.default.connectionString"] = Configuration.ConnectionString;
                    properties["quartz.dataSource.default.provider"] = "SqlServer-20";
                    break;

                default:
                    throw new ArgumentException(string.Format("Unsupported PersistenceStoreType {0}", Configuration.PersistenceStoreType));
            }

            return properties;
        }

        private static void AddCustomTriggerListeners()
        {
            if (Configuration.CustomTriggerListenerAssemblyNames != null)
            {
                foreach (var listenerAssemblyName in Configuration.CustomTriggerListenerAssemblyNames)
                {
                    try
                    {
                        var asm = GetAssembly(listenerAssemblyName);
                        Type listenerType = asm != null
                            ? asm.GetTypes().FirstOrDefault(i => IsOfType(i, typeof (ITriggerListener)))
                            : null;

                        if (listenerType != null)
                        {
                            var listener = (ITriggerListener) Activator.CreateInstance(listenerType);
                            _instance.ListenerManager.AddTriggerListener(listener);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("Error adding TriggerListener from {0}", listenerAssemblyName), ex);
                    }
                }
            }
        }

        private static void AddCustomJobListeners()
        {
            if (Configuration.CustomJobListenerAssemblyNames != null)
            {
                foreach (var listenerAssemblyName in Configuration.CustomJobListenerAssemblyNames)
                {
                    try
                    {
                        var asm = GetAssembly(listenerAssemblyName);
                        Type listenerType = asm != null
                            ? asm.GetTypes().FirstOrDefault(i => IsOfType(i, typeof(IJobListener)))
                            : null;

                        if (listenerType != null)
                        {
                            var listener = (IJobListener)Activator.CreateInstance(listenerType);
                            _instance.ListenerManager.AddJobListener(listener);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("Error adding JobListener from {0}", listenerAssemblyName), ex);
                    }
                }
            }
        }

        private static void AddCustomSchedulerListeners()
        {
            if (Configuration.CustomSchedulerListenerAssemblyNames != null)
            {
                foreach (var listenerAssemblyName in Configuration.CustomSchedulerListenerAssemblyNames)
                {
                    try
                    {
                        var asm = GetAssembly(listenerAssemblyName);
                        Type listenerType = asm != null
                            ? asm.GetTypes().FirstOrDefault(i => IsOfType(i, typeof(ISchedulerListener)))
                            : null;

                        if (listenerType != null)
                        {
                            var listener = (ISchedulerListener)Activator.CreateInstance(listenerType);
                            _instance.ListenerManager.AddSchedulerListener(listener);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("Error adding SchedulerListener from {0}", listenerAssemblyName), ex);
                    }
                }
            }
        }

        private static void AddCustomAuthorization()
        {
            if (!string.IsNullOrEmpty(Configuration.CustomAuthorizationAssemblyName))
            {
                var authorizerAssemblyName = Configuration.CustomAuthorizationAssemblyName;

                try
                {
                    var asm = GetAssembly(authorizerAssemblyName);
                    Type authorizerType = asm != null
                        ? asm.GetTypes().FirstOrDefault(i => IsOfType(i, typeof(IAuthorize)))
                        : null;

                    if (authorizerType != null)
                    {
                        _instance.Context.Add("CustomAuthorizerType", authorizerType);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error adding authorizer from {0}", authorizerAssemblyName), ex);
                }
            }
        }

        private static void AddCustomPermissionsManager()
        {
            if (!string.IsNullOrEmpty(Configuration.CustomPermissionsManagerAssemblyName))
            {
                var permissionsManagerAssemblyName = Configuration.CustomPermissionsManagerAssemblyName;

                try
                {
                    var asm = GetAssembly(permissionsManagerAssemblyName);
                    Type permissionsManagerType = asm != null
                        ? asm.GetTypes().FirstOrDefault(i => IsOfType(i, typeof(IPermissionsManager)))
                        : null;

                    if (permissionsManagerType != null)
                    {
                        _instance.Context.Add("CustomPermissionsManagerType", permissionsManagerType);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error adding permissions manager from {0}", permissionsManagerAssemblyName), ex);
                }
            }
        }

        private static Assembly GetAssembly(string assemblyName)
        {
            if (!assemblyName.ToLower().EndsWith(".dll"))
            {
                assemblyName += ".dll";
            }

            Assembly asm = Assembly.LoadFrom(assemblyName);

            return asm;
        }

        private static bool IsOfType(Type t, Type listenerType)
        {
            if (t == null)
                return false;
            var isListener = t.GetInterface(listenerType.FullName) != null;
            if (!isListener)
                isListener = IsOfType(t.BaseType, listenerType);
            return isListener;
        }
    }
}

