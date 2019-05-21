using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Core
{
    public class PermissionsHelper : IPermissionsHelper
    {
        private readonly IScheduler _scheduler;

        public PermissionsHelper(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IEnumerable<string> GetAuthorizedJobGroups()
        {
            // Return wildcard if no custom permissions manager
            if (!_scheduler.Context.ContainsKey("CustomPermissionsManagerType")) return new List<string> {"*"};

            var permissionsManager = (IPermissionsManager) Activator.CreateInstance(
                (Type) _scheduler.Context["CustomPermissionsManagerType"]);
            return permissionsManager.GetPermittedJobGroups();
        }
    }
}
