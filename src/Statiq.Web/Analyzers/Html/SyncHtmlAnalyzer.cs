using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Statiq.Common;

namespace Statiq.Web
{
    public abstract class SyncHtmlAnalyzer : HtmlAnalyzer
    {
        protected sealed override Task AnalyzeAsync(IHtmlDocument htmlDocument, IDocument document, IAnalyzerContext context)
        {
            Analyze(htmlDocument, document, context);
            return Task.CompletedTask;
        }

        protected abstract void Analyze(IHtmlDocument htmlDocument, IDocument document, IAnalyzerContext context);
    }
}
