using System;
using System.Collections.Generic;
using Quartz;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler
{
    public static class PermissionsHelper
    {
        public static IEnumerable<string> GetAuthorizedJobGroups()
        {
            var scheduler = ObjectFactory.GetInstance<IScheduler>();

            var permissionsManager = scheduler.Context.ContainsKey("CustomPermissionsManagerType")
                ? (IPermissionsManager)Activator.CreateInstance(
                    (Type)scheduler.Context["CustomPermissionsManagerType"])
                : new DefaultPermissionsManager();

            var authorizedJobGroups = permissionsManager.GetPermittedJobGroups();
            return authorizedJobGroups;
        }
    }
}
