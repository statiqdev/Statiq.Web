using System.Collections.Generic;

namespace Wyam.Core.Configuration.Preprocessing
{
    internal class Preprocessor : IPreprocessor
    {
        private readonly Dictionary<string, IDirective> _directives = new Dictionary<string, IDirective>();

        public Preprocessor()
        {
            // Manually add directives instead of using reflection for better startup speed
            AddDirective(new NuGetDirective(), "n", "nuget");
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
    }
}
