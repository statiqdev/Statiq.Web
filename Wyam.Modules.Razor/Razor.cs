using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;

namespace Wyam.Modules.Razor
{
    public class Razor : IModule
    {
        private readonly Type _basePageType;
        private Func<IDocument, string> _viewStartPath;
        private string _ignorePrefix = "_";
        
        public Razor(Type basePageType = null)
        {
            if (basePageType != null && !typeof(BaseRazorPage).IsAssignableFrom(basePageType))
            {
                throw new ArgumentException("The Razor base page type must derive from BaseRazorPage.");
            }
            _basePageType = basePageType;
        }

        public Razor SetViewStart(string path)
        {
            _viewStartPath = x => path;
            return this;
        }

        public Razor SetViewStart(Func<IDocument, string> path)
        {
            _viewStartPath = path;
            return this;
        }

        public Razor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IRazorPageFactory pageFactory = new VirtualPathRazorPageFactory(context.InputFolder, context, _basePageType);

            return inputs
                .Where(x => _ignorePrefix == null || !x.ContainsKey("SourceFileName") || !x.String("SourceFileName").StartsWith(_ignorePrefix))
                .Select(x =>
                {
                    IViewStartProvider viewStartProvider = new ViewStartProvider(pageFactory, _viewStartPath?.Invoke(x));
                    IRazorViewFactory viewFactory = new RazorViewFactory(viewStartProvider);
                    IRazorViewEngine viewEngine = new RazorViewEngine(pageFactory, viewFactory);

                    ViewContext viewContext = new ViewContext(null, new ViewDataDictionary(), null, x.Metadata, context, viewEngine);
                    string relativePath = "/";
                    if (x.ContainsKey("RelativeFilePath"))
                    {
                        relativePath += x.String("RelativeFilePath");
                    }

                    using (context.Trace.WithIndent().Verbose("Processing Razor for {0}", x.Source))
                    {
                        ViewEngineResult viewEngineResult = viewEngine.GetView(viewContext, relativePath, x.Stream).EnsureSuccessful();

                        using (StringWriter writer = new StringWriter())
                        {
                            viewContext.View = viewEngineResult.View;
                            viewContext.Writer = writer;
                            AsyncHelper.RunSync(() => viewEngineResult.View.RenderAsync(viewContext));
                            return x.Clone(writer.ToString());
                        }
                    }
                });
        }
    }
}
