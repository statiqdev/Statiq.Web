using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public abstract class MarkdownAnalyzer : DocumentAnalyzer
    {
        public override string[] Pipelines => new[] { nameof(Web.Pipelines.Content) };

        protected override sealed async Task AnalyzeAsync(IDocument document, IAnalyzerContext context)
        {
            MarkdownDocument markdownDocument = document.Get<MarkdownDocument>(nameof(MarkdownDocument));
            if (markdownDocument is object)
            {
                await AnalyzeAsync(markdownDocument, document, context);
            }
        }

        protected abstract Task AnalyzeAsync(MarkdownDocument markdown, IDocument document, IAnalyzerContext context);
    }
}
