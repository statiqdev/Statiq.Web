using System;
using System.Collections.Generic;
using Wyam.Configuration.NuGet;

namespace Wyam.Configuration.Preprocessing
{
    internal class Preprocessor : IPreprocessor
    {
        private readonly Dictionary<string, IDirective> _directives = new Dictionary<string, IDirective>();

        public void AddDirectives(Configurator configurator)
        {
            AddDirective(new NuGetDirective(configurator.PackageInstaller), "n", "nuget");
            AddDirective(new NuGetSourceDirective(configurator.PackageInstaller), "ns", "nuget-source");
            AddDirective(new NuGetConfigDirective(configurator.PackageInstaller), "nc", "nuget-config");
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
