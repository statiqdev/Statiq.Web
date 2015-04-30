using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // Replaces a search string in the specified content with the contents of the input context
    public class ReplaceIn : ContentModule
    {
        private readonly string _search;

        public ReplaceIn(string search, object content)
            : base(content)
        {
            _search = search;
        }

        public ReplaceIn(string search, Func<IModuleContext, object> content)
            : base(content)
        {
            _search = search;
        }

        public ReplaceIn(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        protected override IEnumerable<IModuleContext> Execute(object content, IModuleContext input, IPipelineContext pipeline)
        {
            if (content == null)
            {
                content = string.Empty;
            }
            if (string.IsNullOrEmpty(_search))
            {
                return new[] { input.Clone(content.ToString()) };
            }
            return new[] { input.Clone(content.ToString().Replace(_search, input.Content)) };
        }
    }
}
