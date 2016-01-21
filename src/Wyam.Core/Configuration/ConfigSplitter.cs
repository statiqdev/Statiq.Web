using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Configuration
{
    internal static class ConfigSplitter
    {
        public static ConfigParts Split(string code)
        {
            List<string> configLines = code.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.None).ToList();

            // Get setup
            int startLine = 1;
            string setup = null;
            int setupLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '=');
            });
            if (setupLine != -1)
            {
                List<string> setupLines = configLines.Take(setupLine).ToList();
                setup = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, setupLines)}";
                startLine = setupLines.Count + 2;
                configLines.RemoveRange(0, setupLine + 1);
            }

            // Get declarations
            string declarations = null;
            int declarationLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '-');
            });
            if (declarationLine != -1)
            {
                List<string> declarationLines = configLines.Take(declarationLine).ToList();
                declarations = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, declarationLines)}";
                startLine += declarationLines.Count + 1;
                configLines.RemoveRange(0, declarationLine + 1);
            }

            // Get config
            string config = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, configLines)}";

            return new ConfigParts(setup, declarations, config);
        }
    }
}
