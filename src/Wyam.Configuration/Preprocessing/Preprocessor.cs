using System;
using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    internal class Preprocessor : IPreprocessor
    {
        private readonly Dictionary<string, IDirective> _directives = new Dictionary<string, IDirective>();

        public void AddDirectives(Configurator configurator)
        {
            AddDirective(new NuGetDirective(configurator.Packages), "n", "nuget");
        }

        private void AddDirective(IDirective directive, params string[] names)
        {
            foreach (string name in names)
            {
                _directives.Add(name, directive);
            }
        }
        
        public bool ContainsDirective(string name)
        {
            return _directives.ContainsKey(name);
        }

        public void ProcessDirectives(IEnumerable<DirectiveUse> uses)
        {
            foreach (DirectiveUse use in uses)
            {
                IDirective directive;
                if (_directives.TryGetValue(use.Name, out directive))
                {
                    try
                    {
                        directive.Process(use.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error while processing directive on line {use.Line}: #{use.Name} {use.Value}{Environment.NewLine}{ex}");
                    }
                }
            }
        }
    }
}
