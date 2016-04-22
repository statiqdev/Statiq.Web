using System.Collections.Generic;

namespace Wyam.Core.Configuration
{
    internal class ConfigParserResult
    {
        public List<KeyValuePair<string, string>> Directives { get; } 
            = new List<KeyValuePair<string, string>>();
        public string Declarations { get; set; }
        public string Body { get; set; }
    }
}