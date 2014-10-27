using System.Collections.Generic;

namespace R.Scheduler.Contracts.DataContracts
{
    public class QueryResponse
    {
        public bool Valid { get; set; }
        public List<Error> Errors { get; set; }
    }
}
