using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using Quartz;
using R.Scheduler.Contracts.JobTypes.AssemblyPlugin.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.AssemblyPlugin.Controllers
{
    public class AssemblyPluginsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        protected AssemblyPluginsController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        // GET api/values 
        [Route("api/plugins")]
        public IEnumerable<PluginJob> Get()
        {
            Logger.Info("Entered AssemblyPluginsController.Get().");

            var jobDetails = _schedulerCore.GetJobDetails(typeof (AssemblyPluginJob));

            return jobDetails.Select(jobDetail =>
                                                    new PluginJob
                                                    {
                                                        JobName = jobDetail.Key.Name,
                                                        JobGroup = jobDetail.Key.Group,
                                                        SchedulerName = _schedulerCore.SchedulerName,
                                                        AssemblyPath = jobDetail.JobDataMap.GetString("pluginPath"),
                                                    }).ToList();

        }

        /// <summary>
        /// Schedules a temporary job for an immediate execution
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/plugins/{jobName}/{jobGroup?}")]
        public QueryResponse Execute(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Execute(). jobName = {0}, jobName = {1}", jobName, jobGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.ExecuteJob(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorExecutingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/plugins/triggers/{jobName}/{jobGroup?}")]
        public QueryResponse Unschedule(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Unschedule(). jobName = {0}, jobName = {1}", jobName, jobGroup);

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

        [AcceptVerbs("POST")]
        [Route("api/plugins")]
        public QueryResponse Post([FromBody]PluginJob model)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Post(). Job Name = {0}", model.JobName);

            var response = new QueryResponse { Valid = true };

            var dataMap = new Dictionary<string, object>
            {
                {"pluginPath", model.AssemblyPath},
            };

            try
            {
                _schedulerCore.CreateJob(model.JobName, model.JobGroup, typeof(AssemblyPluginJob), dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorCreatingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        [Route("api/plugins/triggers/{jobName}/{jobGroup?}")]
        public IList<TriggerDetails> Get(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Get(). jobName = {0}, jobName = {1}", jobName, jobGroup);

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
                            : (DateTime?) null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?) null,
                    PreviousFireTimeUtc =
                        (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?) null,
                    FinalFireTimeUtc = (quartzTrigger.FinalFireTimeUtc.HasValue)
                        ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime
                        : (DateTime?) null,
                    Type = triggerType
                });
            }

            return triggerDetails;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/plugins/{jobName}/{jobGroup?}")]
        public QueryResponse Delete(string jobName, string jobGroup = null)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Delete(). jobName = {0}, jobName = {1}", jobName, jobGroup);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJob(jobName, jobGroup);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorDeletingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/simpleTriggers")]
        public QueryResponse Post([FromBody]CustomJobSimpleTrigger model)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Post(). Name = {0}", model.TriggerName);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.ScheduleTrigger(new SimpleTrigger
                {
                    Name = model.TriggerName,
                    Group = model.TriggerGroup,
                    JobName = model.JobName,
                    JobGroup = model.JobGroup,
                    RepeatCount = model.RepeatCount,
                    RepeatInterval = model.RepeatInterval,
                    StartDateTime = model.StartDateTime,
                });
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


        [AcceptVerbs("POST")]
        [Route("api/plugins/cronTriggers")]
        public QueryResponse Post([FromBody] CustomJobCronTrigger model)
        {
            Logger.InfoFormat("Entered AssemblyPluginsController.Post(). Name = {0}", model.TriggerName);

            var response = new QueryResponse {Valid = true};

            try
            {
                _schedulerCore.ScheduleTrigger(new CronTrigger
                {
                    Name = model.TriggerName,
                    Group = model.TriggerGroup,
                    JobName = model.JobName,
                    JobGroup = model.JobGroup,
                    CronExpression = model.CronExpression,
                    StartDateTime = model.StartDateTime,
                });
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
    }
}
