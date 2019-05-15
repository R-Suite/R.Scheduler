using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Repository.Hierarchy;
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

            // Return wildcard if no custom permissions manager
            if (!scheduler.Context.ContainsKey("CustomPermissionsManagerType")) return new List<string>{"*"};

            var permissionsManager = (IPermissionsManager) Activator.CreateInstance(
                (Type) scheduler.Context["CustomPermissionsManagerType"]);
            return permissionsManager.GetPermittedJobGroups();

        }
    }
}
