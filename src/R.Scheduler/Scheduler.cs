using System;
using Quartz;
using Quartz.Impl;

namespace R.Scheduler
{
    public sealed class Scheduler
    {
        private static IScheduler _instance;
        private static readonly object SyncRoot = new Object();

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
    }
}
