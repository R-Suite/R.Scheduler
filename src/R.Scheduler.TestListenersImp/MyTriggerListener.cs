using System;
using Quartz;

namespace R.Scheduler.TestListenersImp
{
    public class MyTriggerListener : ITriggerListener
    {
        public string Name
        {
            get { return "MyTestTriggerListener"; }
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Console.WriteLine("Trigger Fired...");
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            return false;
        }

        public void TriggerMisfired(ITrigger trigger)
        {
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
        }
    }
}
