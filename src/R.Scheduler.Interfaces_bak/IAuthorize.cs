using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace R.Scheduler.Interfaces
{
    public interface IAuthorize
    {
        List<string> Roles { get; set; }
        List<string> Users { get; set; } 
        bool IsAuthorized(HttpActionContext actionContext);
    }
}
