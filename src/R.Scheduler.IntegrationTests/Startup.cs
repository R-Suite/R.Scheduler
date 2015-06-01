using System.Web.Http;
using Owin;

namespace R.Scheduler.IntegrationTests
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            //config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{test}");

            config.MapHttpAttributeRoutes();
            config.EnsureInitialized(); 
            appBuilder.UseWebApi(config);
        }
    }
}
