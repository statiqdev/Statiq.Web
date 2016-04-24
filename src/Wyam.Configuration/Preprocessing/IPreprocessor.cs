using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    internal interface IPreprocessor
    {
        bool ContainsDirective(string name);
        void ProcessDirectives(IEnumerable<DirectiveUse> uses);
    }
}
