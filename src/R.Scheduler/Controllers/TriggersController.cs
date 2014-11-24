using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using Quartz;
using R.Scheduler.Contracts.Model;
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

        [Route("api/triggers/{jobName}/{jobGroup?}")]
        public IList<TriggerDetails> Get(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered TriggersController.Get(). jobName = {0}, jobName = {1}", jobName, jobGroup);

            IEnumerable<ITrigger> quartzTriggers = _schedulerCore.GetTriggersOfJob(jobName, jobGroup);

            IList<TriggerDetails> triggerDetails = new List<TriggerDetails>();

            foreach (ITrigger quartzTrigger in quartzTriggers)
            {
                var triggerType = string.Empty;
                if (quartzTrigger is ICronTrigger)
                {
                    triggerType = "Cron";
                }
                if (quartzTrigger is ISimpleTrigger)
                {
                    triggerType = "Simple";
                }
                var nextFireTimeUtc = quartzTrigger.GetNextFireTimeUtc();
                var previousFireTimeUtc = quartzTrigger.GetPreviousFireTimeUtc();
                triggerDetails.Add(new TriggerDetails
                {
                    Name = quartzTrigger.Key.Name,
                    Group = quartzTrigger.Key.Group,
                    JobName = quartzTrigger.JobKey.Name,
                    JobGroup = quartzTrigger.JobKey.Group,
                    Description = quartzTrigger.Description,
                    StartTimeUtc = quartzTrigger.StartTimeUtc.UtcDateTime,
                    EndTimeUtc =
                        (quartzTrigger.EndTimeUtc.HasValue)
                            ? quartzTrigger.EndTimeUtc.Value.UtcDateTime
                            : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc =
                        (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue)
                        ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime
                        : (DateTime?)null,
                    Type = triggerType
                });
            }

            return triggerDetails;
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/triggers/{jobName}/{jobGroup?}")]
        public QueryResponse Unschedule(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered TriggersController.Unschedule(). jobName = {0}, jobName = {1}", jobName, jobGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJobTriggers(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorUnschedulingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
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
    }
}
