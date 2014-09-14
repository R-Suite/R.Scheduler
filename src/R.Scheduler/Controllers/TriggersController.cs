using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class TriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public TriggersController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggers")]
        public void DeleteTrigger(string trigger)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTrigger(). trigger = {0}", trigger);

            _schedulerCore.RemoveTrigger(trigger);
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggers")]
        public void DeleteTrigger(string trigger, string triggerGroup)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTrigger(). triggerGroup = {0}, triggerGroup = {1}", trigger, triggerGroup);

            _schedulerCore.RemoveTrigger(trigger, triggerGroup);
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggerGroups")]
        public void DeleteTriggerGroup(string triggerGroup)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTriggerGroup(). triggerGroup = {0}", triggerGroup);

            _schedulerCore.RemoveTriggerGroup(triggerGroup);
        }
    }
}
