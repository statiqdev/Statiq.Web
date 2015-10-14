using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Replaces a search string in the input document content with the specified content
    public class Replace : ContentModule
    {
        private readonly string _search;

        public Replace(string search, object content)
            : base(content)
        {
            _search = search;
        }

        public Replace(string search, ContextConfig content)
            : base(content)
        {
            _search = search;
        }

        public Replace(string search, DocumentConfig content) 
            : base(content)
        {
            _search = search;
        }

        public Replace(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
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
