using System;
using System.Collections.Generic;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Appends the specified content to the existing content
    public class Append : ContentModule
    {
        public Append(object content) 
            : base(content)
        {
        }

        public Append(ContextConfig content)
            : base(content)
        {
        }

        public Append(DocumentConfig content) 
            : base(content)
        {
        }

        public Append(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new [] { content == null ? input : input.Clone(input.Content + content) };
        }
    }
}