using System;
using System.IO;
using System.Threading.Tasks;
using Markdig.Syntax;
using Statiq.Common;
using Statiq.Markdown;
using Statiq.Web.Pipelines;

namespace Statiq.Web
{
    public abstract class MarkdownAnalyzer : Analyzer
    {
        // Cache generated MarkdownDocument instances
        private readonly ConcurrentCache<IContentProvider, Task<MarkdownDocument>> _markdownDocumentCache = new ConcurrentCache<IContentProvider, Task<MarkdownDocument>>();

        // The metadata key where a MarkdownDocument can be found (as set by the RenderMarkdown module)
        private readonly string _markdownDocumentKey;

        protected MarkdownAnalyzer()
            : this(nameof(MarkdownDocument))
        {
        }

        protected MarkdownAnalyzer(string markdownDocumentKey)
        {
            _markdownDocumentKey = markdownDocumentKey;
            PipelinePhases.Add(nameof(AnalyzeContent), Phase.Process);
        }

        public override Task BeforeEngineExecutionAsync(IEngine engine, Guid executionId)
        {
            _markdownDocumentCache.Clear();
            return Task.CompletedTask;
        }

        protected override sealed async Task AnalyzeDocumentAsync(IDocument document, IAnalyzerContext context)
        {
            // Getting the Markdown content is a little tricky, that's why we have to rely on the metadata that the RenderMarkdown module saves
            // Otherwise the document contains rendered HTML by now and there's no way to view the original Markdown
            // If there isn't metadata from the RenderMarkdown module, we can also check to see if the document is still Markdown and parse it with default Markdig extensions if so
            MarkdownDocument markdownDocument = _markdownDocumentKey.IsNullOrEmpty() ? null : document.Get<MarkdownDocument>(_markdownDocumentKey);
            if (markdownDocument is null && document.MediaTypeEquals(MediaTypes.Markdown))
            {
                // Create (or get) a MarkdownDocument and cache it
                markdownDocument = await _markdownDocumentCache.GetOrAdd(
                    document.ContentProvider,
                    async _ => MarkdownHelper.RenderMarkdown(document, await document.GetContentStringAsync(), new StringWriter()));
            }

            if (markdownDocument is object)
            {
                await AnalyzeAsync(markdownDocument, document, context);
            }
        }

        protected abstract Task AnalyzeAsync(MarkdownDocument markdown, IDocument document, IAnalyzerContext context);
    }
}
