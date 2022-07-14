using System;

namespace PWARAbilityTool
{
    public class EmailQueue
    {
        public int AccountId { get; set; }
        public int EmailId { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public int EmailSentFlag { get; set; }
        public DateTime EmailSentTimestamp { get; set; }
        public DateTime EmailCreateTimestamp { get; set; }
        public string EmailToAddress { get; set; }

    }
}
