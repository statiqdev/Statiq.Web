using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // Overwrites the existing content with the specified content
    public class Content : ContentModule
    {
        public Content(object content)
            : base(content)
        {
        }

        public Content(Func<IDocument, object> content)
            : base(content)
        {
        }

        public Content(params IModule[] modules)
            : base(modules)
        {
        }

        public Content(bool forEachDocument, params IModule[] modules)
            : base(forEachDocument, modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IPipelineContext pipeline)
        {
            return new [] { content == null ? input : input.Clone(content.ToString()) };
        }
    }
}
