using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wyam.Configuration.Assemblies;
using Wyam.Configuration.NuGet;

namespace Wyam.Configuration.Preprocessing
{
    internal class Preprocessor : IPreprocessor
    {
        private static readonly ConcurrentDictionary<string, IDirective> _directives 
            = new ConcurrentDictionary<string, IDirective>(StringComparer.OrdinalIgnoreCase);
        private readonly Configurator _configurator;

        static Preprocessor()
        {
            AddDirective(new NuGetDirective());
            AddDirective(new NuGetSourceDirective());
            AddDirective(new NuGetConfigDirective());
            AddDirective(new AssemblyDirective());
            AddDirective(new AssemblyNameDirective());
        }

        private static void AddDirective(IDirective directive)
        {
            foreach (string name in directive.DirectiveNames)
            {
                _directives.TryAdd(name, directive);
            }
        }


        public Preprocessor(Configurator configurator)
        {
            _configurator = configurator;
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
                        string line = use.Line.HasValue ? (" on line " + use.Line.Value) : string.Empty;
                        throw new Exception($"Error while processing directive{line}: #{use.Name} {use.Value}{Environment.NewLine}{ex}");
                    }
                }
            }
        }

        public IEnumerable<IDirective> Directives => _directives.Values.Distinct();
    }
}
