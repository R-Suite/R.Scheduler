namespace R.Scheduler.Contracts.JobTypes.Email.Model
{
    public class EmailJob : BaseJob
    {
        public string CcRecipient { get; set; }
        public string Encoding { get; set; }
        public string Body { get; set; }
        public string Password { get; set; }
        public string Recipient { get; set; }
        public string ReplyTo { get; set; }
        public string Sender { get; set; }
        public string SmtpHost { get; set; }
        public string SmtpPort { get; set; }
        public string Subject { get; set; }
        public string Username { get; set; }
    }
}
