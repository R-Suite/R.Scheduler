using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R.Scheduler.Interfaces
{
    public interface IPermissionsHelper
    {
        IEnumerable<string> GetAuthorizedJobGroups();
    }
}
