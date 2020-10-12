using System;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public class DelegateHtmlAnalyzer : HtmlAnalyzer
    {
        private readonly Func<IHtmlDocument, IDocument, IAnalyzerContext, Task> _analyzeFunc;

        public DelegateHtmlAnalyzer(LogLevel logLevel, Func<IHtmlDocument, IDocument, IAnalyzerContext, Task> analyzeFunc)
        {
            LogLevel = logLevel;
            _analyzeFunc = analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
        }

        protected sealed override async Task AnalyzeAsync(IHtmlDocument htmlDocument, IDocument document, IAnalyzerContext context) => await _analyzeFunc(htmlDocument, document, context);
    }
}
