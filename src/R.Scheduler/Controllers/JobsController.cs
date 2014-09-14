using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class JobsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public JobsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("DELETE")]
        [Route("api/jobs")]
        public void Delete(string jobName)
        {
            Logger.InfoFormat("Entered JobsController.Delete(). jobName = {0}", jobName);

            _schedulerCore.RemoveJob(jobName);
        }
    }
}
