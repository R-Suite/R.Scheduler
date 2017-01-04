using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    [SchedulerAuthorize(AppSettingRoles = "Roles", AppSettingUsers = "Users")]
    public class TriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public TriggersController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all triggers of a specified job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobs/{jobId}/triggers")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<TriggerDetails> Get(Guid jobId)
        {
            Logger.DebugFormat("Entered TriggersController.Get(). jobId = {0}", jobId);

            IDictionary<ITrigger, Guid> quartzTriggers = _schedulerCore.GetTriggersOfJob(jobId);

            return TriggerHelper.GetTriggerDetails(quartzTriggers);
        }

        /// <summary>
        /// Get all triggers of all jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/fireTimes")]
        [SchedulerAuthorize(AppSettingRoles = "Read.Roles", AppSettingUsers = "Read.Users")]
        public IList<TriggerFireTime> Get(DateTime start, DateTime end)
        {
            Logger.Debug("Entered TriggersController.Get()");

            IEnumerable<TriggerFireTime> fireTimes = _schedulerCore.GetFireTimesBetween(start, end);

            return fireTimes as IList<TriggerFireTime>;
        }

        /// <summary>
        /// Schedule SimpleTrigger for a specified job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/simpleTriggers")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody]SimpleTrigger model)
        {
            Logger.InfoFormat("Entered TriggersController.Post(). Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.ScheduleTrigger(model);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = "Server",
                        Message = string.Format("Error scheduling trigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Schedule CronTrigger for a specified job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/cronTriggers")]
        [SchedulerAuthorize(AppSettingRoles = "Create.Roles", AppSettingUsers = "Create.Users")]
        public QueryResponse Post([FromBody] CronTrigger model)
        {
            Logger.DebugFormat("Entered TriggersController.Post(). Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                var id = _schedulerCore.ScheduleTrigger(model);
                response.Id = id;
            }
            catch (Exception ex)
            {
                string type = "Server";

                if (ex is FormatException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = type,
                        Message = string.Format("Error scheduling CronTrigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Remove all triggers of a specified job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/jobs/{jobId}/triggers")]
        [SchedulerAuthorize(AppSettingRoles = "Delete.Roles", AppSettingUsers = "Delete.Users")]
        public QueryResponse Unschedule(Guid jobId)
        {
            Logger.DebugFormat("Entered TriggersController.Unschedule(). jobId = {0}", jobId);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJobTriggers(jobId);
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

        /// <summary>
        /// Remove specified trigger
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/triggers/{id}")]
        [SchedulerAuthorize(AppSettingRoles = "Delete.Roles", AppSettingUsers = "Delete.Users")]
        public QueryResponse DeleteTrigger(Guid id)
        {
            Logger.DebugFormat("Entered TriggersController.DeleteTrigger(). id = {0}", id);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTrigger(id);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing trigger {0}. {1}", id, ex.Message);

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
                        Message = string.Format("Error removing trigger {0}.", id)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Pause or resume specified trigger
        /// </summary>
        /// <param name="id">trigger id</param>
        /// <param name="state">[on, off]</param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/triggers/{id}/state")]
        [SchedulerAuthorize(AppSettingRoles = "Update.Roles", AppSettingUsers = "Update.Users")]
        public QueryResponse ToggleTriggerState(Guid id, [FromBody] string state)
        {
            Logger.DebugFormat("Entered TriggersController.ToggleTrigger(). id = {0}, state = {1}", id, state);

            var response = new QueryResponse { Valid = true, Id = id};

            try
            {
                if (null == state || (state.ToLower() != "on" && state.ToLower() != "off"))
                {
                    throw new Exception("Invalid state. Provide state value 'ON' or 'OFF'");
                }

                if (state.ToLower() == "off")
                    _schedulerCore.PauseTrigger(id);
                else
                    _schedulerCore.ResumeTrigger(id);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error toggling trigger {0}. {1}", id, ex.Message);

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
                        Code = "ErrorTogglingTrigger",
                        Type = type,
                        Message = string.Format("Error toggling trigger state. ({0})", ex.Message)
                    }
                };
            }

            return response;
        }
    }
}
