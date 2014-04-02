using System;
using R.MessageBus.Interfaces;
using R.Scheduler.Contracts.Messages;

namespace R.Scheduler.Handlers
{
    public class ScheduleOneTimeJobCommandHandler : IMessageHandler<ScheduleOneTimeJobCommand>
    {
        public void Execute(ScheduleOneTimeJobCommand message)
        {
            throw new NotImplementedException();
        }
    }
}
