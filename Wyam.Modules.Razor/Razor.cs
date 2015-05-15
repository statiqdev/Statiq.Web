using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Wyam.Extensibility;

namespace Wyam.Modules.Razor
{
    // TODO: Add support for common HtmlHelpers, especially partial views
    public class Razor : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x =>
            {
                // Create ViewContext
                // Create ViewEngine
                // ViewEngine.FindView.EnsureSuccessful()
                // Get RazorView (result.View)
                // RazorView.RenderAsync

                return x.Clone(string.Empty);
            });
        }
    }
}
