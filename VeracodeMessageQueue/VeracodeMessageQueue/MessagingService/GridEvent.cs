using System;
using System.Collections.Generic;
using System.Text;

namespace VeracodeMessageQueue.MessagingService
{
    public class GridEvent
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
    }
}
