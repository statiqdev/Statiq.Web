using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Configuration.Preprocessing;
using Wyam.Core.Configuration;

namespace Wyam.Configuration
{
    // Splits the configuration file into declaration and body sections and finds any directives
    internal static class ConfigParser
    {
        public static ConfigParserResult Parse(string code, IPreprocessor preprocessor)
        {
            ConfigParserResult result = new ConfigParserResult();
            List<string> lines = code.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.None).ToList();

            // Find all directives
            for (int c = 0; c < lines.Count; c++)
            {
                if (lines[c].StartsWith("#"))
                {
                    int firstSpace = lines[c].IndexOf(' ');
                    if (firstSpace != -1)
                    {
                        string directive = lines[c].Substring(1, firstSpace - 1);
                        if (preprocessor.ContainsDirective(directive))
                        {
                            result.DirectiveUses.Add(new DirectiveUse(c + 1, directive, lines[c].Substring(firstSpace + 1).Trim()));
                            lines[c] = "//" + lines[c];  // Just comment out the directive so it still shows in exports
                        }
                    }
                }
            }
            
            // Get declarations
            int startLine = 1;
            int declarationLine = lines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '-');
            });
            if (declarationLine != -1)
            {
                List<string> declarationLines = lines.Take(declarationLine).ToList();
                result.Declarations = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, declarationLines)}";
                startLine += declarationLines.Count + 1;
                lines.RemoveRange(0, declarationLine + 1);
            }

            // Get body
            result.Body = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";

            return result;
        }
    }
}
