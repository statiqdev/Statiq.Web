using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;

namespace Wyam.Modules.Razor
{
    /// <summary>
    /// Parses, compiles, and renders Razor templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Razor is the templating language used by ASP.NET MVC. This module can parse and compile Razor 
    /// templates and then render them to HTML. While a bit 
    /// outdated, <a href="http://haacked.com/archive/2011/01/06/razor-syntax-quick-reference.aspx/">this guide</a> 
    /// is a good quick reference for the Razor language syntax.
    /// </para>
    /// <para>
    /// This module is based on the Razor code in the forthcoming ASP.NET 5 (vNext). 
    /// It was written from the ground-up and doesn't use an intermediate library like RazorEngine 
    /// (which is a great library, implementing directly just provides more control). Note that for 
    /// now, TagHelpers are not implemented in Wyam. Their API is still changing and it would have 
    /// been too difficult to keep up. Support for TagHelpers may be introduced 
    /// once ASP.NET MVC 5 becomes more stable.
    /// </para>
    /// <para>
    /// Whenever possible, the same conventions as the Razor engine in ASP.NET MVC were used. It's 
    /// important to keep in mind however, that this is <em>not</em> ASP.NET MVC. Many features you may 
    /// be used to will not work (like most of the <c>HtmlHelper</c> extensions) and others just don't 
    /// make sense (like the concept of <em>actions</em> and <em>controllers</em>). Also, while property names and 
    /// classes in the two engines have similar names(such as <c>HtmlHelper</c>) they are not the same, 
    /// and code intended to extend the capabilities of Razor in ASP.NET MVC probably won't work. 
    /// That said, a lot of functionality does function the same as it does in ASP.NET MVC.
    /// </para>
    /// </remarks>
    /// <category>Templates</category>
    /// <include file='Documentation.xml' path='/Documentation/Razor/*' />
    public class Razor : IModule
    {
        private readonly Type _basePageType;
        private DocumentConfig _viewStartPath;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content. 
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public Razor(Type basePageType = null)
        {
            if (basePageType != null && !typeof(BaseRazorPage).IsAssignableFrom(basePageType))
            {
                throw new ArgumentException("The Razor base page type must derive from BaseRazorPage.");
            }
            _basePageType = basePageType;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module.
        /// </summary>
        /// <param name="path">The path to the alternate ViewStart file.</param>
        public Razor WithViewStart(string path)
        {
            _viewStartPath = (doc, ctx) => path;
            return this;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. This 
        /// lets you specify a different ViewStart file for each document. For example, you could return a 
        /// ViewStart based on document location or document metadata. Returning <c>null</c> from the 
        /// function reverts back to the default ViewStart search behavior for that document.
        /// </summary>
        /// <param name="path">A delegate that should return the ViewStart path as a <c>string</c>, 
        /// or <c>null</c> for the default ViewStart search behavior.</param>
        public Razor WithViewStart(DocumentConfig path)
        {
            _viewStartPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a file prefix to ignore. If a document has a metadata value for <c>SourceFileName</c> and 
        /// that metadata value starts with the specified prefix, that document will not be processed or 
        /// output by the module. By default, the Razor module ignores all documents prefixed with 
        /// an underscore (_). Specifying <c>null</c> will result in no documents being ignored.
        /// </summary>
        /// <param name="prefix">The file prefix to ignore.</param>
        public Razor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IRazorPageFactory pageFactory = new VirtualPathRazorPageFactory(context.InputFolder, context, _basePageType);
            List<IDocument> validInputs = inputs
                .Where(x => _ignorePrefix == null 
                    || !x.ContainsKey(Keys.SourceFileName) 
                    || !x.FilePath(Keys.SourceFileName).FullPath.StartsWith(_ignorePrefix))
                .ToList();

            // Compile the pages in parallel
            ConcurrentDictionary<IDocument, Tuple<ViewContext, ViewEngineResult>> compilationResults
                = new ConcurrentDictionary<IDocument, Tuple<ViewContext, ViewEngineResult>>();
            Parallel.ForEach(validInputs, x =>
            {
                Trace.Verbose("Compiling Razor for {0}", x.Source);
                IViewStartProvider viewStartProvider = new ViewStartProvider(pageFactory, _viewStartPath?.Invoke<string>(x, context));
                IRazorViewFactory viewFactory = new RazorViewFactory(viewStartProvider);
                IRazorViewEngine viewEngine = new RazorViewEngine(pageFactory, viewFactory);
                ViewContext viewContext = new ViewContext(null, new ViewDataDictionary(), null, x, context, viewEngine);
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
                .Select(input =>
                {
                    using (Trace.WithIndent().Verbose("Processing Razor for {0}", input.Source))
                    {
                        Tuple<ViewContext, ViewEngineResult> compilationResult;
                        if (compilationResults.TryGetValue(input, out compilationResult))
                        {
                            using (StringWriter writer = new StringWriter())
                            {
                                compilationResult.Item1.View = compilationResult.Item2.View;
                                compilationResult.Item1.Writer = writer;
                                Task.Factory.StartNew(() => compilationResult.Item2.View.RenderAsync(compilationResult.Item1),
                                    cancellationToken, TaskCreationOptions.None, exclusiveScheduler).Unwrap().GetAwaiter().GetResult();
                                return context.GetDocument(input, writer.ToString());
                            }
                        }
                        Trace.Warning("Could not find compilation result for {0}", input.Source);
                        return null;
                    }
                });
        }

        private string GetRelativePath(IDocument document)
        {
            string relativePath = "/";
            if (document.ContainsKey(Keys.RelativeFilePath))
            {
                relativePath += document.String(Keys.RelativeFilePath);
            }
            return relativePath;
        }
    }
}
