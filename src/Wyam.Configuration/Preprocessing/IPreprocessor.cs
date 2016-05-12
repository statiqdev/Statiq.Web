using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    public interface IPreprocessor
    {
        bool ContainsDirective(string name);
        IEnumerable<IDirective> Directives { get; }
        void AddValue(DirectiveValue value);
    }
}
