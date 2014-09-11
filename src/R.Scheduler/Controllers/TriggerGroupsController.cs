using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class TriggerGroupsController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public TriggerGroupsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggerGroups")]
        public void Delete(string triggerGroup)
        {
            Logger.InfoFormat("Entered TriggerGroupsController.Delete(). triggerGroup = {0}", triggerGroup);

            _schedulerCore.RemoveTriggerGroup(triggerGroup);
        }
    }
}
