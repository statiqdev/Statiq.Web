using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration
{
    internal class ConfigParserResult
    {
        public List<DirectiveUse> DirectiveUses { get; } = new List<DirectiveUse>();
        public string Declarations { get; set; }
        public string Body { get; set; }
    }
}