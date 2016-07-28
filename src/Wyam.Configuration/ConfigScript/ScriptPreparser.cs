using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.ConfigScript
{
    // Finds and comments out any directives
    internal class ScriptPreparser
    {
        private readonly IPreprocessor _preprocessor;

        public ScriptPreparser(IPreprocessor preprocessor)
        {
            _preprocessor = preprocessor;
        }

        public List<DirectiveValue> DirectiveValues { get; } = new List<DirectiveValue>();

        public string Code { get; private set; } = string.Empty;

        public void Parse(string code)
        {
            DirectiveValues.Clear();
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
                        if (_preprocessor.ContainsDirective(directive))
                        {
                            DirectiveValues.Add(new DirectiveValue(c + 1, directive, lines[c].Substring(firstSpace + 1).Trim()));
                        }
                        lines[c] = "//" + lines[c];  // Just comment out the directive so it still shows in script code exports and influences the line number
                    }
                }
            }

            Code = string.Join(Environment.NewLine, lines);
        }
    }
}
