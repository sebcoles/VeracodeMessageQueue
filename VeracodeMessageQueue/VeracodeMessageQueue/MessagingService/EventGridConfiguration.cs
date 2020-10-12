using System;
using System.Collections.Generic;
using System.Text;

namespace VeracodeMessageQueue.MessagingService
{
    public class EventGridConfiguration
    {
        public string VeracodeTopicEndpoint { get; set; }
        public string SharedAccessKey { get; set; }
    }
}
