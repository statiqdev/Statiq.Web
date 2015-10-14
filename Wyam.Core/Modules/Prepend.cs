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
    // Prepends the specified content to the existing content
    public class Prepend : ContentModule
    {
        public Prepend(object content)
            : base(content)
        {
        }

        public Prepend(ContextConfig content)
            : base(content)
        {
        }

        public Prepend(DocumentConfig content) 
            : base(content)
        {
        }

        public Prepend(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new[] { content == null ? input : input.Clone(content + input.Content) };
        }
    }
}
