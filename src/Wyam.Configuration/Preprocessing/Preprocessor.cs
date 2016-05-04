using System;
using System.Collections.Generic;
using Wyam.Configuration.Assemblies;
using Wyam.Configuration.NuGet;

namespace Wyam.Configuration.Preprocessing
{
    internal class Preprocessor : IPreprocessor
    {
        private readonly Dictionary<string, IDirective> _directives = new Dictionary<string, IDirective>(StringComparer.OrdinalIgnoreCase);
        private readonly Configurator _configurator;

        public Preprocessor(Configurator configurator)
        {
            _configurator = configurator;
        }

        public void AddDirectives()
        {
            AddDirective(new NuGetDirective());
            AddDirective(new NuGetSourceDirective());
            AddDirective(new NuGetConfigDirective());
            AddDirective(new AssemblyDirective());
            AddDirective(new AssemblyNameDirective());
        }

        private void AddDirective(IDirective directive)
        {
            foreach (string name in directive.DirectiveNames)
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
                        directive.Process(_configurator, use.Value);
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
