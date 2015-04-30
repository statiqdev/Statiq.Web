using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // Replaces a search string in the input content with the specified content
    public class Replace : ContentModule
    {
        private readonly string _search;

        public Replace(string search, object content)
            : base(content)
        {
            _search = search;
        }

        public Replace(string search, Func<IModuleContext, object> content)
            : base(content)
        {
            _search = search;
        }

        public Replace(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        protected override IEnumerable<IModuleContext> Execute(object content, IModuleContext input, IPipelineContext pipeline)
        {
            if (string.IsNullOrEmpty(_search))
            {
                return new[] { input };
            }
            if (content == null)
            {
                content = string.Empty;
            }
            return new[] { input.Clone(input.Content.Replace(_search, content.ToString())) };
        }
    }
}
