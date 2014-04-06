using System;
using Quartz;
using Quartz.Impl;
using R.Scheduler.Contracts.Interfaces;
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
        /// Sets implementation of IPluginStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetPluginStore<T>() where T : class, IPluginStore
        {
            ObjectFactory.Configure(x => x.For<IPluginStore>().Use<T>());
        }
    }
}
