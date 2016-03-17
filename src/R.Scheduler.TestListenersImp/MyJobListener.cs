using System;
using Quartz;

namespace R.Scheduler.TestListenersImp
{
    public class MyJobListener : IJobListener
    {
        public void JobToBeExecuted(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return "MyTestJobListener"; }
        }
    }
}
