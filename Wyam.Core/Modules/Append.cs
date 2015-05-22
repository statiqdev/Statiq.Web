using System;
using System.Collections.Generic;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // Appends the specified content to the existing content
    public class Append : ContentModule
    {
        public Append(object content) 
            : base(content)
        {
        }

        public Append(Func<IDocument, object> content) 
            : base(content)
        {
        }

        public Append(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IPipelineContext pipeline)
        {
            return new [] { content == null ? input : input.Clone(input.Content + content) };
        }
    }
}