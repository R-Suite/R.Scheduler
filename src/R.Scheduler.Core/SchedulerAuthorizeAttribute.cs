using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using Common.Logging;
using Quartz;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Core
{
    public class SchedulerAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string AppSettingUsers { get; set; }
        public string AppSettingRoles { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            Logger.DebugFormat("Entered SchedulerAuthorizeAttribute.IsAuthorized");

            var scheduler = ObjectFactory.GetInstance<IScheduler>();

            if (scheduler.Context.ContainsKey("CustomAuthorizer"))
            {
                var users = new List<string>();
                var roles = new List<string>();

                if (!string.IsNullOrEmpty(Users))
                {
                    users.AddRange(Users.Split());
                }
                if (!string.IsNullOrEmpty(Roles))
                {
                    roles.AddRange(Roles.Split());
                }
                char[] sep = {','};
                if (!string.IsNullOrEmpty(AppSettingRoles) && ConfigurationManager.AppSettings[AppSettingRoles] != null)
                {
                    roles.AddRange(ConfigurationManager.AppSettings[AppSettingRoles].Split(sep, StringSplitOptions.RemoveEmptyEntries));
                }
                if (!string.IsNullOrEmpty(AppSettingUsers) && ConfigurationManager.AppSettings[AppSettingUsers] != null)
                {
                    users.AddRange(ConfigurationManager.AppSettings[AppSettingUsers].Split(sep, StringSplitOptions.RemoveEmptyEntries));
                }

                var authorize = (IAuthorize)scheduler.Context["CustomAuthorizer"];
                authorize.Roles = roles;
                authorize.Users = users;

                var isAuthorized =  authorize.IsAuthorized(actionContext);

                Logger.DebugFormat("Custom authorization enabled. isAuthorized = {0}", isAuthorized);

                return isAuthorized;
            }

            Logger.Debug("Custom authorization not enabled.");

            // Authorized by default
            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized"
            };

            throw new HttpResponseException(msg);
        }
    }
}
