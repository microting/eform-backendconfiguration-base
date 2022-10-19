using System;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities
{
    public class Email : PnBase
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyType { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string Sent { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime DelayedUntil { get; set; }
    }
}