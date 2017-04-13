using System;
using System.Collections.Generic;

namespace R.Scheduler.Contracts.Model
{
    public class QueryResponse
    {
        public bool Valid { get; set; }
        public List<Error> Errors { get; set; }
        public Guid Id { get; set; }
    }
}
