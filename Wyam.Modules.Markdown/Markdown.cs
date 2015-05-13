using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Modules.Markdown
{
    public class Markdown : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(CommonMark.CommonMarkConverter.Convert(x.Content)));
        }
    }
}
