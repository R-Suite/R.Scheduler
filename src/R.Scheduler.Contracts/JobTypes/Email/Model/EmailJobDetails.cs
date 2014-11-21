using System.Collections.Generic;
using R.Scheduler.Contracts.Model;

namespace R.Scheduler.Contracts.JobTypes.Email.Model
{
    public class EmailJobDetails
    {
        public string Name { get; set; }

        public IList<TriggerDetails> TriggerDetails { get; set; }
    }
}
