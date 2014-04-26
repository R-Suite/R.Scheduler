using System;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using R.Scheduler.Contracts.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;

namespace R.Scheduler
{
    public sealed class Scheduler
    {
        private static IScheduler _instance;
        private static readonly object SyncRoot = new Object();

        static Scheduler()
        {
            ObjectFactory.Initialize(x=>x.AddRegistry<SmRegistry>());
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
                        ISchedulerFactory schedFact = new StdSchedulerFactory();
                        _instance = schedFact.GetScheduler();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// Sets implementation of IPluginStore in IoC Container. 
        /// (Must be called before Scheduler.Instance())
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void SetPluginStore(PluginStoreType pluginStoreType)
        {
            if (null != _instance)
            {
                throw new Exception("PluginStore cannot be set after the scheduler has been initialized.");
            }

            switch (pluginStoreType)
            {
                case PluginStoreType.InMemory:
                    ObjectFactory.Configure(x => x.For<IPluginStore>().Use<InMemoryPluginStore>());
                    break;
                case PluginStoreType.Postgre:
                    ObjectFactory.Configure(x => x.For<IPluginStore>().Use<PostgrePluginStore>());
                    break;
                default:
                    throw new Exception(string.Format("Unsupported pluginStoreType {0}", pluginStoreType));
            }
        }
    }
}
