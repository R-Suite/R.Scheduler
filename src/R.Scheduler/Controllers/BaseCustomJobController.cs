using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using Quartz;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public abstract class BaseCustomJobController : ApiController
    {
        private readonly ICustomJobStore _repository;
        readonly ISchedulerCore _schedulerCore;
        readonly IJobTypeManager _jobTypeManager;

        protected BaseCustomJobController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
            _jobTypeManager = ObjectFactory.GetInstance<IJobTypeManager>();
        }

        protected ICustomJob GetRegisteredCustomJob(string id, string jobType)
        {
            ICustomJob registeredJob = null;

            // Try to get plugin by id
            Guid guidId;
            if (Guid.TryParse(id, out guidId))
            {
                registeredJob = _repository.GetRegisteredJob(guidId);
            }

            // Couldn't get it by id, try by name
            if (null == registeredJob)
            {
                registeredJob = _repository.GetRegisteredJob(id, jobType);
            }

            return registeredJob;
        }

        protected QueryResponse ExecuteCustomJob(string model, string dataMapParamKey, Type jobType)
        {
            ICustomJob registeredJob = GetRegisteredCustomJob(model, jobType.Name);

            var response = new QueryResponse { Valid = true };
            if (null == registeredJob)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("{0} not found", model)
                    }
                };

                return response;
            }

            var dataMap = new Dictionary<string, object> { { dataMapParamKey, registeredJob.Params } };

            try
            {
                _schedulerCore.ExecuteJob(jobType, dataMap);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorTriggeringCustomJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }
            return response;
        }

        protected QueryResponse DescheduleCustomJob(string model, Type jobType)
        {
            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTriggersOfJobType(jobType);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingTriggersOfJobType",
                        Type = "Server",
                        Message = string.Format("Error:{0}", ex.Message)
                    }
                };
            }

            return response;
        }

        protected QueryResponse RegisterCustomJob(ICustomJob model)
        {
            var response = new QueryResponse { Valid = true };

            try
            {
                _jobTypeManager.Register(model.Name, model.Params);
            }
            catch (Exception ex)
            {
                string type = "Server";
                if (ex is FileNotFoundException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRegisteringCustomJob",
                        Type = type,
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        protected QueryResponse UpdateCustomJob(string id, ICustomJob model)
        {
            var response = new QueryResponse { Valid = true };

            try
            {
                _repository.UpdateName(new Guid(id), model.Name);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorUpdatingCustomJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }
            return response;
        }

        protected IList<TriggerDetails> GetCustomJobTriggerDetails(Type jobType)
        {
            var quartzTriggers = _schedulerCore.GetTriggersOfJobType(jobType);

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
                        (quartzTrigger.EndTimeUtc.HasValue) ? quartzTrigger.EndTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    NextFireTimeUtc = (nextFireTimeUtc.HasValue) ? nextFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    PreviousFireTimeUtc =
                        (previousFireTimeUtc.HasValue) ? previousFireTimeUtc.Value.UtcDateTime : (DateTime?)null,
                    FinalFireTimeUtc =
                        (quartzTrigger.FinalFireTimeUtc.HasValue)
                            ? quartzTrigger.FinalFireTimeUtc.Value.UtcDateTime
                            : (DateTime?)null,
                    Type = triggerType
                });
            }

            return triggerDetails;
        }

        protected QueryResponse DeleteCustomJob(string id, Type jobType)
        {
            var registeredJob = GetRegisteredCustomJob(id, jobType.Name);

            var response = new QueryResponse { Valid = true };

            _schedulerCore.RemoveTriggersOfJobType(jobType);

            int result = _repository.Remove(registeredJob.Id);

            if (result == 0)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("{0} not found", id)
                    }
                };
            }

            return response;
        }

        protected QueryResponse CreateCustomJobSimpleTrigger(string id, CustomJobSimpleTrigger model, string dataMapParamKey, Type jobType)
        {
            var response = new QueryResponse { Valid = true };

            ICustomJob registeredCustomJob = GetRegisteredCustomJob(id, jobType.Name);

            if (null == registeredCustomJob)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("Error loading registered  {0} Job {1}", jobType.Name, model.TriggerName)
                    }
                };

                return response;
            }

            try
            {
                _schedulerCore.ScheduleTrigger(new SimpleTrigger
                {
                    Name = model.TriggerName,
                    Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : registeredCustomJob.Id + "_Group",
                    JobName = !string.IsNullOrEmpty(model.JobName) ? model.JobName : registeredCustomJob.Id + "_JobName",
                    JobGroup = registeredCustomJob.Id.ToString(),
                    RepeatCount = model.RepeatCount,
                    RepeatInterval = model.RepeatInterval,
                    StartDateTime = model.StartDateTime,
                    DataMap = new Dictionary<string, object> { { dataMapParamKey, registeredCustomJob.Params } }
                }, jobType);
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

        protected QueryResponse CreateCustomJobCronTrigger(string id, CustomJobCronTrigger model, string dataMapParamKey, Type jobType)
        {
            var response = new QueryResponse { Valid = true };

            ICustomJob registeredPlugin = GetRegisteredCustomJob(id, jobType.Name);

            if (null == registeredPlugin)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "RegisteredCustomJobNotFound",
                        Type = "Sender",
                        Message = string.Format("Error loading registered CustomJob {0}", model.TriggerName)
                    }
                };

                return response;
            }

            try
            {
                _schedulerCore.ScheduleTrigger(new CronTrigger
                {
                    Name = model.TriggerName,
                    Group = !string.IsNullOrEmpty(model.TriggerGroup) ? model.TriggerGroup : registeredPlugin.Id + "_Group",
                    JobName = !string.IsNullOrEmpty(model.JobName) ? model.JobName : registeredPlugin.Id + "_JobName",
                    JobGroup = registeredPlugin.Id.ToString(),
                    CronExpression = model.CronExpression,
                    StartDateTime = model.StartDateTime,
                    DataMap = new Dictionary<string, object> { { dataMapParamKey, registeredPlugin.Params } }
                }, jobType);
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
