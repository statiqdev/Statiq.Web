using System.Linq;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public class FencedCodeBlocksShouldHaveLanguage : SyncMarkdownAnalyzer
    {
        public override LogLevel LogLevel { get; set; } = LogLevel.None;

        protected override void Analyze(MarkdownDocument markdown, IDocument document, IAnalyzerContext context)
        {
            foreach (FencedCodeBlock block in markdown.Descendants<FencedCodeBlock>().Where(b => b.Info.IsNullOrEmpty()))
            {
                context.AddAnalyzerResult(document, $"Line {block.Line}: Fenced code blocks should specify a language");
            }
        }
    }
}
