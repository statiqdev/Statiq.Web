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

        public Content(Func<IModuleContext, object> content)
            : base(content)
        {
        }

        public Content(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IModuleContext> Execute(object content, IModuleContext input, IPipelineContext pipeline)
        {
            return new [] { content == null ? input : input.Clone(content.ToString()) };
        }
    }
}
