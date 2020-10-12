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
    public class FencedCodeBlocksShouldHaveLanguage : SyncMarkdownAnalyzer
    {
        public override LogLevel LogLevel { get; set; } = LogLevel.Warning;

        protected override void Analyze(MarkdownDocument markdown, IDocument document, IAnalyzerContext context)
        {
            foreach (FencedCodeBlock block in markdown.Descendants<FencedCodeBlock>().Where(b => b.Info.IsNullOrEmpty()))
            {
                context.AddAnalyzerResult(document, $"Line {block.Line}: Fenced code blocks should specify a language");
            }
        }
    }
}
