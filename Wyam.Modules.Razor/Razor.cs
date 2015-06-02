using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Wyam.Core;
using Wyam.Abstractions;
using Wyam.Core.Helpers;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;

namespace Wyam.Modules.Razor
{
    // TODO: Add support for common HtmlHelpers, especially partial views
    public class Razor : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IRazorPageFactory pageFactory = new VirtualPathRazorPageFactory(context.InputFolder);
            IViewStartProvider viewStartProvider = new ViewStartProvider(pageFactory);
            IRazorViewFactory viewFactory = new RazorViewFactory(viewStartProvider);
            IRazorViewEngine viewEngine = new RazorViewEngine(pageFactory, viewFactory);

            return inputs.Select(x =>
            {
                ViewContext viewContext = new ViewContext(null, null)
                {
                    Metadata = x.Metadata,
                    ExecutionContext = context
                };
                string relativePath = "/";
                if (x.Metadata.ContainsKey("FilePath"))
                {
                    relativePath += PathHelper.GetRelativePath(context.InputFolder, (string) x["FilePath"]);
                }
                ViewEngineResult viewEngineResult = viewEngine.GetView(viewContext, relativePath, x.Content).EnsureSuccessful();
                using (StringWriter writer = new StringWriter())
                {
                    viewContext.View = viewEngineResult.View;
                    viewContext.Writer = writer;
                    AsyncHelper.RunSync(() => viewEngineResult.View.RenderAsync(viewContext));
                    return x.Clone(writer.ToString());
                }
            });
        }
    }
}
