using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Modules.Razor
{
    public class Razor : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x =>
            {
                
                return x.Clone(string.Empty);
            });
        }
    }
}
