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
    public class Razor : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IRazorPageFactory pageFactory = new VirtualPathRazorPageFactory(context.InputFolder, context);
            IViewStartProvider viewStartProvider = new ViewStartProvider(pageFactory);
            IRazorViewFactory viewFactory = new RazorViewFactory(viewStartProvider);
            IRazorViewEngine viewEngine = new RazorViewEngine(pageFactory, viewFactory);

            return inputs.Select(x =>
            {
                ViewContext viewContext = new ViewContext(null, new ViewDataDictionary(), null, x.Metadata, context, viewEngine);
                string relativePath = "/";
                if (x.Metadata.ContainsKey("FileRelative"))
                {
                    relativePath += x.Get<string>("FileRelative");
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
