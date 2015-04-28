using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    // Overwrites the existing content with the specified content
    public class Content : IModule
    {
        private readonly Func<IModuleContext, string> _content;

        public Content(string content)
        {
            _content = x => content;
        }

        public Content(Func<IModuleContext, string> content)
        {
            _content = content ?? (x => null);
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(_content(x)));
        }
    }
}
