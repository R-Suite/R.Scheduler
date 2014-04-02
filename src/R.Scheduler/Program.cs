using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace R.Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // get a scheduler
            IScheduler sched = Scheduler.Instance();
            sched.Start();
        }
    }
}
