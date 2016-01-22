using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Configuration
{
    internal class ConfigParts
    {
        public string Setup { get; }
        public string Declarations { get; }
        public string Config { get; }

        public ConfigParts(string setup, string declarations, string config)
        {
            Setup = setup;
            Declarations = declarations;
            Config = config;
        }
    }
}
