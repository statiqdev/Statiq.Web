using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Syntax;
using Statiq.Common;

namespace Statiq.Web
{
    public abstract class SyncMarkdownAnalyzer : MarkdownAnalyzer
    {
        protected sealed override Task AnalyzeAsync(MarkdownDocument markdown, IDocument document, IAnalyzerContext context)
        {
            Analyze(markdown, document, context);
            return Task.CompletedTask;
        }

        protected abstract void Analyze(MarkdownDocument markdown, IDocument document, IAnalyzerContext context);
    }
}
