using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common;
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

        public Razor WithViewStart(string path)
        {
            _viewStartPath = x => path;
            return this;
        }

        public Razor WithViewStart(Func<IDocument, string> path)
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
            List<IDocument> validInputs = inputs
                .Where(x => _ignorePrefix == null || !x.ContainsKey("SourceFileName") || !x.String("SourceFileName").StartsWith(_ignorePrefix))
                .ToList();

            // Compile the pages in parallel
            ConcurrentDictionary<IDocument, Tuple<ViewContext, ViewEngineResult>> compilationResults
                = new ConcurrentDictionary<IDocument, Tuple<ViewContext, ViewEngineResult>>();
            Parallel.ForEach(validInputs, x =>
            {
                context.Trace.Verbose("Compiling Razor for {0}", x.Source);
                IViewStartProvider viewStartProvider = new ViewStartProvider(pageFactory, _viewStartPath?.Invoke(x));
                IRazorViewFactory viewFactory = new RazorViewFactory(viewStartProvider);
                IRazorViewEngine viewEngine = new RazorViewEngine(pageFactory, viewFactory);
                ViewContext viewContext = new ViewContext(null, new ViewDataDictionary(), null, x.Metadata, context, viewEngine);
                ViewEngineResult viewEngineResult;
                using (Stream stream = x.GetStream())
                {
                    viewEngineResult = viewEngine.GetView(viewContext, GetRelativePath(x), stream).EnsureSuccessful();
                }
                compilationResults[x] = new Tuple<ViewContext, ViewEngineResult>(viewContext, viewEngineResult);
            });

            // Now evaluate them in sequence - have to do this because BufferedHtmlContent doesn't appear to work well in multi-threaded parallel execution
            TaskScheduler exclusiveScheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            CancellationToken cancellationToken = new CancellationToken();
            return validInputs
                .Select(x =>
                {
                    using (context.Trace.WithIndent().Verbose("Processing Razor for {0}", x.Source))
                    {
                        Tuple<ViewContext, ViewEngineResult> compilationResult;
                        if (compilationResults.TryGetValue(x, out compilationResult))
                        {
                            using (StringWriter writer = new StringWriter())
                            {
                                compilationResult.Item1.View = compilationResult.Item2.View;
                                compilationResult.Item1.Writer = writer;
                                Task.Factory.StartNew(() => compilationResult.Item2.View.RenderAsync(compilationResult.Item1), 
                                    cancellationToken, TaskCreationOptions.None, exclusiveScheduler).Unwrap().GetAwaiter().GetResult();
                                return x.Clone(writer.ToString());
                            }
                        }
                        context.Trace.Warning("Could not find compilation result for {0}", x.Source);
                        return null;
                    }
                });
        }

        private string GetRelativePath(IDocument document)
        {
            string relativePath = "/";
            if (document.ContainsKey("RelativeFilePath"))
            {
                relativePath += document.String("RelativeFilePath");
            }
            return relativePath;
        }
    }
}
