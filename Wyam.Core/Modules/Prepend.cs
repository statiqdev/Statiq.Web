using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    // Prepends the specified content to the existing content
    public class Prepend : IModule
    {
        private readonly Func<IModuleContext, object> _content;

        public Prepend(object content)
        {
            _content = x => content;
        }

        public Prepend(Func<IModuleContext, object> content)
        {
            _content = content ?? (x => null);
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(_content(x).ToString() + x.Content));
        }
    }
}
