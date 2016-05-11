using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    public interface IPreprocessor
    {
        bool ContainsDirective(string name);
        void ProcessDirectives(IEnumerable<DirectiveUse> uses);
        IEnumerable<IDirective> Directives { get; }
    }
}
