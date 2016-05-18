using System.Collections.Generic;
using System.Web.Http.Controllers;
using R.Scheduler.Interfaces;

namespace R.Scheduler.TestCustomAuthorizationImp
{
    public class CustomAuthorizer : IAuthorize
    {
        public List<string> Roles { get; set; }
        public List<string> Users { get; set; }
        public bool IsAuthorized(HttpActionContext actionContext)
        {
            // Probably want to grab token/claims from the current principal
            // Redirect to some sort of authentication service if the token is missing or expired.

            // If valid token, ensure the claims contain of the Roles or Users specified


            return true;
        }
    }
}
