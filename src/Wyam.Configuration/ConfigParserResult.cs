using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration
{
    internal class ConfigParserResult
    {
        public List<DirectiveValue> DirectiveValues { get; } = new List<DirectiveValue>();
        public string Declarations { get; set; }
        public string Body { get; set; }
    }
}