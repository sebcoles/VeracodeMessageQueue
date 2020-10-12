using CommandLine;
using CommandLine.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeracodeMessageQueue
{
    [Verb("run", HelpText = "Check differences and raise events")]
    public class RunOptions
    {
        [Option('a', "all", Default = false, Required = false, HelpText = "Run against all Apps for the account")]
        public bool All { get; set; }
    }
}
