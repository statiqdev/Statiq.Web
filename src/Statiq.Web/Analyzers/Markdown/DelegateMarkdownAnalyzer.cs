using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public class DelegateMarkdownAnalyzer : MarkdownAnalyzer
    {
        private readonly Func<MarkdownDocument, IDocument, IAnalyzerContext, Task> _analyzeFunc;

        public DelegateMarkdownAnalyzer(LogLevel logLevel, Func<MarkdownDocument, IDocument, IAnalyzerContext, Task> analyzeFunc)
        {
            LogLevel = logLevel;
            _analyzeFunc = analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
        }

        protected override async Task AnalyzeAsync(MarkdownDocument markdown, IDocument document, IAnalyzerContext context) => await _analyzeFunc(markdown, document, context);
    }
}
