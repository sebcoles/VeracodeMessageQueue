using System;
using System.Collections.Generic;
using System.Text;

namespace VeracodeMessageQueue.MessagingService
{
    public enum MessageTypes
    {
        AppEvent = 1,
        BuildEvent = 2,
        MitigationEvent = 3,
        FlawEvent = 4
    }
}
