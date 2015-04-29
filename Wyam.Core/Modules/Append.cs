using System;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Core.Modules
{
    // Appends the specified content to the existing content
    public class Append : IModule
    {
        private readonly Func<IModuleContext, object> _content;

        public Append(object content)
        {
            _content = x => content;
        }

        public Append(Func<IModuleContext, object> content)
        {
            _content = content ?? (x => null);
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(x.Content + _content(x).ToString()));
        }
    }
}