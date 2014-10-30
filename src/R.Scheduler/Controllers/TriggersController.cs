using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
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
        public QueryResponse DeleteTrigger(string trigger)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTrigger(). trigger = {0}", trigger);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTrigger(trigger);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing trigger  {0}. {1}", trigger, ex.Message);

                string type = "Server";
                if (ex is ArgumentException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingTrigger",
                        Type = type,
                        Message = string.Format("Error removing trigger {0}.", trigger)
                    }
                };
            }

            return response;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggers")]
        public QueryResponse DeleteTrigger(string trigger, string triggerGroup)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTrigger(). triggerGroup = {0}, triggerGroup = {1}", trigger, triggerGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTrigger(trigger, triggerGroup);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing trigger  {0}. {1}", trigger, ex.Message);

                string type = "Server";
                if (ex is ArgumentException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingTrigger",
                        Type = type,
                        Message = string.Format("Error removing trigger {0}.", trigger)
                    }
                };
            }

            return response;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/triggerGroups")]
        public QueryResponse DeleteTriggerGroup(string triggerGroup)
        {
            Logger.InfoFormat("Entered TriggersController.DeleteTriggerGroup(). triggerGroup = {0}", triggerGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTriggerGroup(triggerGroup);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing trigger group  {0}. {1}", triggerGroup, ex.Message);

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingTrigger",
                        Type = "Server",
                        Message = string.Format("Error removing trigger {0}.", triggerGroup)
                    }
                };
            }


            return response;
        }
    }
}
