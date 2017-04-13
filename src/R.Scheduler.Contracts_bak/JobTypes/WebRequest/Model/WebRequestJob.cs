using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R.Scheduler.Contracts.JobTypes.WebRequest.Model
{
    public class WebRequestJob : BaseJob
    {
        public string ActionType { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
    }
}
