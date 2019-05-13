using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    public class DefaultPermissionsManager : IPermissionsManager
    {
        public IEnumerable<string> GetPermittedJobGroups()
        {
            return new List<string>() {"*"};
        }
    }
}
