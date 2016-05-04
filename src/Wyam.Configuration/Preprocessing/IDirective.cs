using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    internal interface IDirective
    {
        IEnumerable<string> DirectiveNames { get; }
        void Process(Configurator configurator, string value);
    }
}
