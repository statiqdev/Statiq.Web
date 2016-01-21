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

        public bool HasSetup => !string.IsNullOrWhiteSpace(Setup);
        public bool HasDeclarations => !string.IsNullOrWhiteSpace(Declarations);

        public ConfigParts()
        {
        }

        public ConfigParts(string setup, string declarations, string config)
        {
            Setup = setup;
            Declarations = declarations;
            Config = config;
        }
    }
}
