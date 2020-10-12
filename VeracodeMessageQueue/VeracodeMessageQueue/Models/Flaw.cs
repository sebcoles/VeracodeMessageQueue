using System;
using System.Collections.Generic;
using System.Text;

namespace VeracodeMessageQueue.Models
{
    public class Flaw
    {
        public string AppId { get; set; }
        public string Id { get; set; }
        public string RemediationStatus { get; set; }
        public string MitigationStatus { get; set; }
    }
}
