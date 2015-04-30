using System;
using System.Collections.Generic;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // Appends the specified content to the existing content
    public class Append : ContentModule
    {
        public Append(object content) 
            : base(content)
        {
        }

        public Append(Func<IModuleContext, object> content) 
            : base(content)
        {
        }

        public Append(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IModuleContext> Execute(object content, IModuleContext input, IPipelineContext pipeline)
        {
            return new [] { content == null ? input : input.Clone(input.Content + content) };
        }
    }
}