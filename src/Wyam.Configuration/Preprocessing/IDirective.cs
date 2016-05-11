using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    public interface IDirective
    {
        IEnumerable<string> DirectiveNames { get; }
        void Process(Configurator configurator, string value);
        string GetHelpText();
    }
}
