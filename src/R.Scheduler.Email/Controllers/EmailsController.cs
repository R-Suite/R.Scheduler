using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using Newtonsoft.Json;
using Quartz.Job;
using R.Scheduler.Contracts.JobTypes.Email.Model;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Controllers;
using R.Scheduler.Interfaces;
using R.Scheduler.Persistance;
using StructureMap;

namespace R.Scheduler.Email.Controllers
{
    public class EmailsController : BaseCustomJobController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ICustomJobStore _repository;

        public EmailsController()
        {
            _repository = ObjectFactory.GetInstance<ICustomJobStore>();
        }

        // GET api/values 
        [Route("api/emails")]
        public IEnumerable<EmailJob> Get()
        {
            Logger.Info("Entered EmailsController.Get().");

            IList<ICustomJob> registeredJobs = _repository.GetRegisteredJobs(typeof(SendMailJob).Name);

            var retval = new List<EmailJob>();
            foreach (var registeredJob in registeredJobs)
            {
                var emailJob = JsonConvert.DeserializeObject<EmailJob>(registeredJob.Params);
                retval.Add(emailJob);
            }

            return retval;
        }

        /// <summary>
        /// Schedules a temporary job for an immediate execution
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/emails/execute")]
        public QueryResponse Execute([FromBody]string model)
        {
            Logger.InfoFormat("Entered EmailsController.Execute(). name = {0}", model);

            ICustomJob registeredJob = GetRegisteredCustomJob(model, typeof(SendMailJob).Name);
            var dataMap = GetDataMap(registeredJob);

            return ExecuteCustomJob(model, dataMap, typeof(SendMailJob));
        }

        /// <summary>
        /// Removes all triggers.
        /// </summary>
        /// <param name="model">Plugin name</param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/emails/deschedule")]
        public QueryResponse Deschedule([FromBody]string model)
        {
            Logger.InfoFormat("Entered EmailsController.Deschedule(). name = {0}", model);

            return DescheduleCustomJob(model, typeof(SendMailJob));
        }

        [AcceptVerbs("POST")]
        [Route("api/emails")]
        public QueryResponse Post([FromBody]EmailJob model)
        {
            Logger.InfoFormat("Entered EmailsController.Post(). Name = {0}", model.Name);

            return RegisterCustomJob(new CustomJob { Name = model.Name, Params = JsonConvert.SerializeObject(model) , JobType = typeof(SendMailJob).Name });
        }

        [AcceptVerbs("PUT")]
        [Route("api/emails/{id}")]
        public QueryResponse Put(string id, [FromBody]EmailJob model)
        {
            Logger.InfoFormat("Entered EmailsController.Put(). name = {0}", model.Name);

            return UpdateCustomJob(id, new CustomJob { Name = model.Name });
        }

        [Route("api/emails/{id}")]
        public EmailJobDetails Get(string id)
        {
            Logger.InfoFormat("Entered EmailsController.Get(). id = {0}", id);

            ICustomJob registeredJob = base.GetRegisteredCustomJob(id, typeof(SendMailJob).Name);

            if (null == registeredJob)
            {
                Logger.ErrorFormat("Error getting registered EmailJob {0}", id);
                return null;
            }

            var retval = new EmailJobDetails
            {
                Name = registeredJob.Name,
                TriggerDetails = new List<TriggerDetails>()
            };

            retval.TriggerDetails = GetCustomJobTriggerDetails(registeredJob);

            return retval;
        }

        [AcceptVerbs("DELETE")]
        [Route("api/emails")]
        public QueryResponse Delete(string id)
        {
            Logger.InfoFormat("Entered EmailsController.Delete(). id = {0}", id);

            return DeleteCustomJob(id, typeof(SendMailJob));
        }

        [AcceptVerbs("POST")]
        [Route("api/emails/{id}/simpleTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobSimpleTrigger model)
        {
            Logger.InfoFormat("Entered EmailsController.Post(). Name = {0}", model.TriggerName);

            ICustomJob registeredJob = GetRegisteredCustomJob(id, typeof(SendMailJob).Name);
            var dataMap = GetDataMap(registeredJob);

            return CreateCustomJobSimpleTrigger(id, model, dataMap, typeof(SendMailJob));
        }


        [AcceptVerbs("POST")]
        [Route("api/emails/{id}/cronTriggers")]
        public QueryResponse Post(string id, [FromBody]CustomJobCronTrigger model)
        {
            Logger.InfoFormat("Entered EmailsController.Post(). Name = {0}", model.TriggerName);

            ICustomJob registeredJob = GetRegisteredCustomJob(id, typeof(SendMailJob).Name);
            var dataMap = GetDataMap(registeredJob);

            return CreateCustomJobCronTrigger(id, model, dataMap, typeof(SendMailJob));
        }

        private static Dictionary<string, object> GetDataMap(ICustomJob registeredJob)
        {
            var emailJob = JsonConvert.DeserializeObject<EmailJob>(registeredJob.Params);

            var dataMap = new Dictionary<string, object>();
            dataMap.Add("smtp_host", emailJob.SmtpHost);
            dataMap.Add("smtp_port", emailJob.SmtpPort);
            dataMap.Add("smtp_username", emailJob.Username);
            dataMap.Add("smtp_password", emailJob.Password);
            dataMap.Add("recipient", emailJob.Recipient);
            dataMap.Add("cc_recipient", emailJob.CcRecipient);
            dataMap.Add("sender", emailJob.Sender);
            dataMap.Add("reply_to", emailJob.ReplyTo);
            dataMap.Add("subject", emailJob.Subject);
            dataMap.Add("message", emailJob.Body);
            dataMap.Add("encoding", emailJob.Encoding);
            return dataMap;
        }    
    }
}
